using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizer.Models;
using Quizer.Models.Lobbies;
using Quizer.Services.Lobbies;
using Quizer.Services.Quizzes;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Quizer.Models.Quizzes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.Http.HttpResults;
using Quizer.Models.User;
using FluentResults;
using Quizer.Exceptions.Services;
using Quizer.Services.Qr;
using Microsoft.AspNetCore.Http.Extensions;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Data.SqlClient;
using static Quizer.Services.Lobbies.ILobbyConductService;
using Quizer.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;


namespace Quizer.Controllers
{
    [Route("[controller]")]
    public class LobbyController(
        ILogger<LobbyController> logger, ILobbyControlService lobbyControlService,
        ILobbyConductService lobbyConductService, ILobbyAuthService lobbyAuthService, ILobbyStatsService lobbyStatsService,
        IQrService qrService, ITempUserService tempUserService, UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, IHubContext<LobbyHub, ILobbyClient> hubContext) : Controller
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobbyGuid"></param>
        /// <param name="error"></param>
        /// <returns>Join view with lobbyGuid in ViewData, or NoFreeSlot or Unavailable views in the case of error</returns>
        [HttpGet("Join/{lobbyGuid}")]
        public IActionResult Join(string lobbyGuid, string? error = null)
        {
            if (error != null)
            {
                if (error == "NoFreeSlot")
                {
                    return View("NoFreeSlot");
                }
                else if (error == "Unavailable")
                {
                    return View("Unavailable");
                }
            }

            ViewData["lobbyGuid"] = lobbyGuid;

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobbyGuid"></param>
        /// <param name="connectionId">SignalR connection ID</param>
        /// <param name="displayName">User chosen display name</param>
        /// <returns>
        /// Pass the Connection ID from the client side: https://stackoverflow.com/a/50394456/15028432
        /// If all ok redirects to Briefing.
        /// If lobby is unavailable returns Unavailable view.
        /// If no free slot (lobby reached max participators count) returns NoFreeSlot view.
        /// </returns>
        [HttpPost("JoinConfirm/{lobbyGuid}")]
        public async Task<IActionResult> JoinConfirm(string lobbyGuid, string connectionId, [FromForm] string displayName)
        {
            ApplicationUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                Result<ApplicationUser> resultUser = await tempUserService.CreateTempUser(displayName);

                if (resultUser.IsSuccess) {
                    user = resultUser.Value;
                    await signInManager.SignInAsync(user, new AuthenticationProperties { IsPersistent = true, AllowRefresh = true });
                }
                else
                {
                    return StatusCode(500);
                }
            }

            Result result = await lobbyControlService.JoinUserAsync(lobbyGuid, user.Id);

            if (result.IsFailed && user.IsTemporal)
            {
                await userManager.DeleteAsync(user);
            }

            if (result.HasError<LobbyUnavailableError>())
            {
                return RedirectToAction("Join", new { lobbyGuid, error = "Unavailable" }); ;
            }
            if (result.HasError<MaxParticipatorsError>())
            {
                return RedirectToAction("Join", new { lobbyGuid, error = "NoFreeSlot" }); ;
            }
            if (result.HasError<UserAlreadyInLobbyError>())
            {
                return Conflict();
            }
            if (result.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }
            if (result.HasError<UserNotFoundError>())
            {
                return StatusCode(500);
            }

            await AddToLobbyGroup(lobbyGuid, connectionId);
            user.HubConnectionId = connectionId;
            await userManager.UpdateAsync(user);

            return RedirectToAction("Briefing", new { lobbyGuid });
        }

        /// <summary>
        /// Kicks the user from the lobby.
        /// </summary>
        /// <param name="lobbyGuid"></param>
        /// <returns></returns>
        [Authorize(Policy = "ParticipatorRights")]
        [HttpPost("Leave/{lobbyGuid}")]
        public async Task<IActionResult> Leave(string lobbyGuid)
        {
            ApplicationUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result result = await lobbyControlService.KickUserAsync(lobbyGuid, user.Id);

            if (result.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }

            if (user.HubConnectionId != null)
            {
                await RemoveFromLobbyGroup(lobbyGuid, user.HubConnectionId);
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobbyGuid"></param>
        /// <returns></returns>
        [Authorize(Policy = "ParticipatorRights")]
        [HttpGet("Briefing/{lobbyGuid}")]
        public async Task<IActionResult> Briefing(string lobbyGuid)
        {
            ApplicationUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobbyGuid"></param>
        /// <returns>Lobby status (Briefing, Game, Results)</returns>
        [HttpGet("LobbyStatus/{lobbyGuid}")]
        public IActionResult Status(string lobbyGuid)
        {
            Result<LobbyStatus> result = lobbyConductService.GetLobbyStatus(lobbyGuid);

            if (result.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }
            if (result.IsFailed)
            {
                return StatusCode(500);
            }

            string status = result.Value.ToString();

            return Ok(status);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobbyGuid"></param>
        /// <returns>Returns GameMaster or GameParticipator view with current question view model</returns>
        [Authorize(Policy = "ParticipatorRights")]
        [HttpGet("Game/{lobbyGuid}")]
        public async Task<IActionResult> Game(string lobbyGuid)
        {
            ApplicationUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result<bool> resultParticipatorCheck = await lobbyAuthService.IsUserParticipator(user.Id, lobbyGuid);
            Result<bool> resultMasterCheck = await lobbyAuthService.IsUserMaster(user.Id, lobbyGuid);

            if (resultParticipatorCheck.HasError<UserNotFoundError>() || resultMasterCheck.HasError<UserNotFoundError>())
            {
                return Unauthorized();
            }
            if (resultParticipatorCheck.HasError<LobbyNotFoundError>() || resultMasterCheck.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }

            if (!resultParticipatorCheck.Value && !resultMasterCheck.Value) {
                return new ForbidResult();
            }

            Result<Question> result = lobbyConductService.GetCurrentQuestion(lobbyGuid);

            if (result.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }
            if (result.HasError<QuizNotFoundError>())
            {
                return NotFound();
            }
            if (result.HasError<LobbyUnavailableError>())
            {
                return Forbid();
            }

            QuestionViewModel viewModel = GetQuestionViewModel(result.Value);

            if (resultMasterCheck.Value)
            { 
                return View("GameMaster", viewModel);
            } else
            {
                return View("GameParticipator", viewModel);
            }
        }

        /// <summary>
        /// Registers user answer
        /// </summary>
        /// <param name="lobbyGuid"></param>
        /// <param name="answerGuid">Should be VALID answer GUID or null in case of test answer, otherwise error will occur,
        /// or just answer or null in the case of text and numerical answer</param>
        /// <returns></returns>
        [Authorize(Policy = "ParticipatorRights")]
        [HttpPost("RegisterAnswer/{lobbyGuid}")]
        public async Task<IActionResult> RegisterAnswer(string lobbyGuid, string? answerGuid)
        {
            ApplicationUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result<bool> resultCheck = await lobbyAuthService.IsUserParticipator(user.Id, lobbyGuid);
            if (resultCheck.HasError<UserNotFoundError>())
            {
                return Unauthorized();
            }
            if (resultCheck.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }

            if (!resultCheck.Value)
            {
                return new ForbidResult();
            }

            Result<Question> questionResult = lobbyConductService.GetCurrentQuestion(lobbyGuid);

            Result result = await lobbyConductService.RegisterTestAnswer(user.Id, lobbyGuid, answerGuid);

            if (result.IsFailed)
            {
                return StatusCode(500);
            }

            return Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobbyGuid"></param>
        /// <param name="answerGuid"></param>
        /// <returns>TestResult, TextResult or NumericalResult view with QuestionResultViewModel between the questions, 
        /// or QuizResults view with StatsViewModel</returns>
        [Authorize(Policy = "ParticipatorRights")]
        [HttpGet("Result/{lobbyGuid}")]
        public async Task<IActionResult> Result(string lobbyGuid)
        {
            ApplicationUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result<bool> resultCheck = await lobbyAuthService.IsUserParticipator(user.Id, lobbyGuid);
            if (resultCheck.HasError<UserNotFoundError>())
            {
                return Unauthorized();
            }
            if (resultCheck.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }
            if (!resultCheck.Value)
            {
                return new ForbidResult();
            }

            Result<LobbyStatus> lobbyStatusResult = lobbyConductService.GetLobbyStatus(lobbyGuid);
            if (lobbyStatusResult.Value == LobbyStatus.Result)
            {
                return View("QuizResults", GetStatsViewModel(lobbyGuid));
            }

            Result<Question> questionResult = lobbyConductService.GetCurrentQuestion(lobbyGuid);
            if (questionResult.HasError<LobbyUnavailableError>())
            {
                return Forbid();
            }

            Question question = questionResult.Value;

            Result<QuestionResultViewModel> result = await GetQuestionResultViewModel(question, user.Id, lobbyGuid);

            if (result.HasError<LobbyAccessDeniedError>())
            {
                return Forbid();
            }

            QuestionResultViewModel viewModel = result.Value;

            if (question.Type == QuestionType.Test)
            {
                return View("TestResult", viewModel);
            } 
            else if (question.Type == QuestionType.TextEntry)
            {
                return View("TextResult", viewModel);
            }
            else if (question.Type == QuestionType.NumberEntry)
            {
                return View("NumericalResult", viewModel);
            } else
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Creates lopbby.
        /// </summary>
        /// <param name="quizGuid"></param>
        /// <param name="maxParticipators"></param>
        /// <returns>Redirects to /Manage</returns>
        [Authorize(Policy = "MemberRights")]
        [HttpPost("Create")]
        public async Task<IActionResult> Create(string quizGuid, int maxParticipators)
        {
            ApplicationUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (quizGuid == null || maxParticipators == 0) {
                return BadRequest();
            }

            Result<string> result = await lobbyControlService.CreateAsync(user.Id, quizGuid, (int)maxParticipators);

            if (result.IsFailed)
            {
                return StatusCode(500);
            }

            Uri uri = new Uri($"{Request.Scheme}://{Request.Host}/Lobby/Join/{result.Value}");
            qrService.GenerateQrCode(result.Value, uri.ToString());

            lobbyConductService.SubscribeToLobbyStatusUpdateEvent(result.Value, async (status) => {
                if (status == LobbyStatus.Question)
                {
                    await hubContext.Clients.Group("lobby_" + result.Value).RedirectToQuestion();
                }
                else if (status == LobbyStatus.Answering)
                {
                    await hubContext.Clients.Group("lobby_" + result.Value).SendAnswer();
                }
                else if (status == LobbyStatus.Break)
                {
                    await hubContext.Clients.Group("lobby_" + result.Value).RedirectToBreak();
                }
                else if (status == LobbyStatus.Result)
                {
                    await hubContext.Clients.Group("lobby_" + result.Value).RedirectToResult();
                }
            });

            return RedirectToAction("Manage", new { lobbyGuid = result.Value });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobbyGuid"></param>
        /// <returns>Manage view with lobbyGuid in ViewData</returns>
        [Authorize(Policy = "MemberRights")]
        [HttpGet("Manage/{lobbyGuid}")]
        public async Task<IActionResult> Manage(string lobbyGuid)
        {
            ApplicationUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result<bool> resultCheck = await lobbyAuthService.IsUserMaster(user.Id, lobbyGuid);
            if (resultCheck.HasError<UserNotFoundError>())
            {
                return Unauthorized();
            }
            if (resultCheck.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }

            if (!resultCheck.Value)
            {
                return new ForbidResult();
            }

            ViewData["lobbyGuid"] = lobbyGuid;
            
            return View();
        }

        /// <summary>
        /// Needed to get lobby join QR coded link.
        /// </summary>
        /// <param name="lobbyGuid"></param>
        /// <returns>QR image file</returns>
        [Authorize(Policy = "MemberRights")]
        [HttpGet("Qr/{lobbyGuid:guid}")]
        public IActionResult Qr(string lobbyGuid)
        {
            Result<byte[]> result = qrService.GetQrByName(lobbyGuid);

            if (result.IsFailed)
            {
                return NotFound();
            }

            return File(result.Value, "image/png");
        }

        /// <summary>
        /// Starts the lobby. Available only for game master.
        /// </summary>
        /// <param name="lobbyGuid"></param>
        /// <returns></returns>
        [Authorize(Policy = "MemberRights")]
        [HttpPost("Start/{lobbyGuid}")]
        public async Task<IActionResult> Start(string lobbyGuid)
        {
            ApplicationUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result<bool> resultCheck = await lobbyAuthService.IsUserMaster(user.Id, lobbyGuid);
            if (resultCheck.HasError<UserNotFoundError>())
            {
                return Unauthorized();
            }
            if (resultCheck.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }
            if (!resultCheck.Value)
            {
                return new ForbidResult();
            }

            Result result = await lobbyControlService.StartLobbyAsync(lobbyGuid);

            if (result.HasError<UserNotFoundError>())
            {
                return StatusCode(500);
            }
            if (result.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }

            return Ok();
        }

        /// <summary>
        /// Stops the lobby. This method removes lobby permanently. Available only for game master.
        /// </summary>
        /// <param name="lobbyGuid"></param>
        /// <returns></returns>
        [Authorize(Policy = "MemberRights")]
        [HttpPost("Stop/{lobbyGuid}")]
        public async Task<IActionResult> Stop(string lobbyGuid)
        {
            ApplicationUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result<bool> resultCheck = await lobbyAuthService.IsUserMaster(user.Id, lobbyGuid);
            if (resultCheck.HasError<UserNotFoundError>())
            {
                return Unauthorized();
            }
            if (resultCheck.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }
            if (!resultCheck.Value)
            {
                return new ForbidResult();
            }

            Result resultUsers = await tempUserService.DeleteLobbyTempUsers(lobbyGuid);
            Result result = lobbyControlService.StopLobby(lobbyGuid);

            if (result.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }

            return StatusCode(418);
        }

        /// <summary>
        /// Kicks the user from the lobby. Available only for game master.
        /// </summary>
        /// <param name="lobbyGuid"></param>
        /// <param name="userToKickId"></param>
        /// <returns></returns>
        [Authorize(Policy = "ParticipatorRights")]
        [HttpPost("Kick/{lobbyGuid}")]
        public async Task<IActionResult> Kick(string lobbyGuid, string userToKickId)
        {
            ApplicationUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result<bool> resultCheck = await lobbyAuthService.IsUserMaster(user.Id, lobbyGuid);
            if (resultCheck.HasError<UserNotFoundError>())
            {
                return Unauthorized();
            }
            if (resultCheck.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }
            if (!resultCheck.Value)
            {
                return new ForbidResult();
            }

            Result result = await lobbyControlService.KickUserAsync(lobbyGuid, userToKickId);

            if (result.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }

            await RemoveUserFromLobbyGroupById(lobbyGuid, userToKickId);

            return Ok();
        }

        /// <summary>
        /// Forces next question. Available only for game master.
        /// </summary>
        /// <param name="lobbyGuid"></param>
        /// <returns></returns>
        [Authorize(Policy = "MemberRights")]
        [HttpPost("ForceNextQuestion/{lobbyGuid}")]
        public async Task<IActionResult> ForceNextQuestion(string lobbyGuid)
        {
            ApplicationUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result<bool> resultCheck = await lobbyAuthService.IsUserMaster(user.Id, lobbyGuid);
            if (resultCheck.HasError<UserNotFoundError>())
            {
                return Unauthorized();
            }
            if (resultCheck.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }
            if (!resultCheck.Value)
            {
                return new ForbidResult();
            }

            lobbyControlService.ForceNextQuestionAsync(lobbyGuid); // ignore result, because the only error it can return is LobbyNotFound

            return Ok();
        }

        private async Task<StatsViewModel> GetStatsViewModel(string lobbyGuid)
        {
            StatsViewModel statsViewModel = new StatsViewModel();

            var result = await lobbyControlService.GetUsersInLobby(lobbyGuid);

            foreach (ApplicationUser lobbyUser in result.Value)
            {
                var pointsResult = await lobbyStatsService.GetUserPoints(lobbyUser.Id, lobbyGuid);
                statsViewModel.UserPoints.Add(lobbyUser.DisplayName != null ? lobbyUser.DisplayName : "null", pointsResult.Value);
            }

            return statsViewModel;
        }

        private async Task<Result<QuestionResultViewModel>> GetQuestionResultViewModel(Question question, string userId, string lobbyGuid)
        {
            Result<IEnumerable<ParticipatorAnswer>> participatorAnswersResult = await lobbyStatsService.GetUserAnswers(userId, lobbyGuid);
            if (participatorAnswersResult.HasError<LobbyAccessDeniedError>())
            {
                return FluentResults.Result.Fail(participatorAnswersResult.Errors.First());
            }
            IEnumerable<ParticipatorAnswer> participatorAnswers = participatorAnswersResult.Value;

            ParticipatorAnswer participatorAnswer = (
                from pA in participatorAnswers where pA.Question.Guid == question.Guid select pA).First();

            QuestionViewModel viewModel = GetQuestionViewModel(question);

            return new QuestionResultViewModel(viewModel, participatorAnswer);
        }

        private QuestionViewModel GetQuestionViewModel(Question Question)
        {
            QuestionViewModel questionViewModel = new()
            {
                Guid = Question.Guid,
                Position = Question.Position,
                Title = Question.Title,
            };

            foreach (Answer a in Question.Answers)
            {
                questionViewModel.Answers.Add(GetAnswerViewModel(a));
            }

            return questionViewModel;
        }

        private AnswerViewModel GetAnswerViewModel(Answer answer)
        {
            return new AnswerViewModel()
            {
                Guid = answer.Guid,
                Title = answer.Title,
                IsCorrect = answer.IsCorrect,
            };
        }

        private async Task AddToLobbyGroup(string lobbyGuid, string connectionId)
        {
            await hubContext.Groups.AddToGroupAsync(connectionId, "lobby_" + lobbyGuid);
        }

        private async Task RemoveFromLobbyGroup(string lobbyGuid, string connectionId)
        {
            await hubContext.Groups.RemoveFromGroupAsync(connectionId, "lobby_" + lobbyGuid);
        }

        private async Task RemoveUserFromLobbyGroupById(string lobbyGuid, string userId)
        {
            ApplicationUser? user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return;
            }

            if (user.HubConnectionId == null)
            {
                return;
            }

            await hubContext.Groups.RemoveFromGroupAsync(user.HubConnectionId, "lobby_" + lobbyGuid);
        }
    }
}
