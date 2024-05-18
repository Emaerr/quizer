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


namespace Quizer.Controllers
{
    [Route("[controller]")]
    public class LobbyController : Controller
    {
        private readonly ILogger<LobbyController> _logger;
        private readonly ILobbyControlService _lobbyControlService;
        private readonly ILobbyAuthService _lobbyAuthService;
        ILobbyConductService _lobbyConductService;
        private readonly IQrService _qrService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LobbyController(ILogger<LobbyController> logger, ILobbyControlService lobbyControlService, ILobbyConductService lobbyConductService, ILobbyAuthService lobbyAuthService, IQrService qrService, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _lobbyControlService = lobbyControlService;
            _lobbyConductService = lobbyConductService;
            _lobbyAuthService = lobbyAuthService;
            _qrService = qrService;
            _userManager = userManager;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobbyGuid"></param>
        /// <returns>
        /// If all ok redirects to Briefing.
        /// If lobby is unavailable returns Unavailable view.
        /// If no free slot (lobby reached max participators count) returns NoFreeSlot view.
        /// </returns>
        [Authorize(Policy = "ParticipatorRights")]
        [HttpGet("Join/{lobbyGuid}")]
        public async Task<IActionResult> Join(string lobbyGuid)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result result = await _lobbyControlService.JoinUserAsync(lobbyGuid, user.Id);

            if (result.HasError<LobbyUnavailableError>())
            {
                return View("Unavailable");
            }
            if (result.HasError<MaxParticipatorsError>())
            {
                return View("NoFreeSlot"); ;
            }
            if (result.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }
            if (result.HasError<UserNotFoundError>())
            {
                return StatusCode(500);
            }

            return RedirectToAction("Briefing", new { lobbyGuid });
        }

        public async Task<IActionResult> Leave(string lobbyGuid)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result result = await _lobbyControlService.KickUserAsync(lobbyGuid, user.Id);

            if (result.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Policy = "ParticipatorRights")]
        [HttpGet("Briefing/{lobbyGuid}")]
        public async Task<IActionResult> Briefing(string lobbyGuid)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            return View();
        }

        [HttpGet("LobbyStatus/{lobbyGuid}")]
        public IActionResult Status(string lobbyGuid)
        {
            Result<LobbyStatus> result = _lobbyConductService.GetLobbyStatus(lobbyGuid);

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
        /// <returns>Returns current question view model</returns>
        [Authorize(Policy = "ParticipatorRights")]
        [HttpGet("Game/{lobbyGuid}")]
        public async Task<IActionResult> Game(string lobbyGuid)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result<bool> resultCheck = await _lobbyAuthService.IsUserParticipator(user.Id, lobbyGuid);
            if (resultCheck.HasError<UserNotFoundError>())
            {
                return Unauthorized();
            } 
            if (resultCheck.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }

            if (!resultCheck.Value) {
                return new ForbidResult();
            }

            Result<QuestionData> result = _lobbyConductService.GetCurrentQuestion(lobbyGuid);

            if (result.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }
            if (result.HasError<QuizNotFoundError>())
            {
                return NotFound();
            }

            QuestionViewModel viewModel = GetQuestionViewModel(result.Value);

            return View(viewModel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobbyGuid"></param>
        /// <param name="answerGuid"></param>
        /// <returns>Question view model, guid of user answer and is this answer correct</returns>
        /// <example><code>@model (QuestionViewModel question, string userAnswerGuid, bool isUserAnswerCorrect)</code></example>
        [Authorize(Policy = "ParticipatorRights")]
        [HttpGet("Result/{lobbyGuid}")]
        public async Task<IActionResult> Result(string lobbyGuid, string? answerGuid)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result<bool> resultCheck = await _lobbyAuthService.IsUserParticipator(user.Id, lobbyGuid);
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

            Result result = await _lobbyConductService.RegisterTestAnswer(user.Id, lobbyGuid, answerGuid);

            if (result.IsFailed)
            {
                return StatusCode(500);
            }

            Result<IEnumerable<AnswerData>> rightAnswers = _lobbyConductService.GetRightAnswers(lobbyGuid);

            Result<QuestionData> questionResult = _lobbyConductService.GetCurrentQuestion( lobbyGuid);

            if (result.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }
            if (result.HasError<QuizNotFoundError>())
            {
                return NotFound();
            }
 
            bool isAnswerCorrect = false;

            foreach (AnswerData answerData in questionResult.Value.Answers)
            {
                if (answerData.Info.IsCorrect)
                {
                    isAnswerCorrect = true;
                }
            }

            QuestionViewModel viewModel = GetQuestionViewModel(questionResult.Value);

            var model = (viewModel, answerGuid, isAnswerCorrect);

            return View(model);
        }

        /// <summary>
        /// Creates lopbby.
        /// </summary>
        /// <param name="quizGuid"></param>
        /// <param name="maxParticipators"></param>
        /// <returns>Redirects to /Manage</returns>
        [Authorize(Policy = "MemberRights")]
        [HttpGet("Create")]
        public async Task<IActionResult> Create(string quizGuid, int maxParticipators)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (quizGuid == null || maxParticipators == 0) {
                return BadRequest();
            }

            Result<string> result = await _lobbyControlService.CreateAsync(user.Id, quizGuid, (int)maxParticipators);

            if (result.IsFailed)
            {
                return StatusCode(500);
            }

            Uri uri = new Uri($"{Request.Scheme}://{Request.Host}/Lobby/Join/{result.Value}");

            _qrService.GenerateQrCode(result.Value, uri.ToString());

            return RedirectToAction("Manage", new { lobbyGuid = result.Value });
        }

        [Authorize(Policy = "MemberRights")]
        [HttpGet("Manage/{lobbyGuid}")]
        public async Task<IActionResult> Manage()
        {
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
            Result<byte[]> result = _qrService.GetQrByName(lobbyGuid);

            if (result.IsFailed)
            {
                return NotFound();
            }

            return File(result.Value, "image/png");
        }

        /// <summary>
        /// Starts the lobby.
        /// </summary>
        /// <param name="lobbyGuid"></param>
        /// <returns></returns>
        [Authorize(Policy = "MemberRights")]
        [HttpGet("Start/{lobbyGuid}")] // TODO: Make it POST
        public async Task<IActionResult> Start(string lobbyGuid)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result<bool> resultCheck = await _lobbyAuthService.IsUserMaster(user.Id, lobbyGuid);
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

            Result result = await _lobbyControlService.StartLobbyAsync(lobbyGuid);

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

        [Authorize(Policy = "MemberRights")]
        [HttpGet("Stop/{lobbyGuid}")] // TODO: Make it POST
        public async Task<IActionResult> Stop(string lobbyGuid)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result<bool> resultCheck = await _lobbyAuthService.IsUserMaster(user.Id, lobbyGuid);
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

            Result result = await _lobbyControlService.StopLobbyAsync(lobbyGuid);

            if (result.HasError<UserNotFoundError>())
            {
                return StatusCode(500);
            }
            if (result.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }

            return StatusCode(418);
        }

        [Authorize(Policy = "ParticipatorRights")]
        [HttpPost("Kick/{lobbyGuid}")]
        public async Task<IActionResult> Kick(string lobbyGuid, string userToKickId)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result<bool> resultCheck = await _lobbyAuthService.IsUserMaster(user.Id, lobbyGuid);
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

            Result result = await _lobbyControlService.KickUserAsync(lobbyGuid, userToKickId);


            if (result.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }

            return Ok();
        }

        private QuestionViewModel GetQuestionViewModel(QuestionData questionData)
        {
            QuestionViewModel questionViewModel = new()
            {
                Guid = questionData.Guid,
                Position = questionData.Info.Position,
                Title = questionData.Info.Title,
            };

            foreach (AnswerData aData in questionData.Answers)
            {
                questionViewModel.Answers.Add(GetAnswerViewModel(aData));
            }

            return questionViewModel;
        }

        private AnswerViewModel GetAnswerViewModel(AnswerData answerData)
        {
            return new AnswerViewModel()
            {
                Guid = answerData.Guid,
                Title = answerData.Info.Title,
                IsCorrect = answerData.Info.IsCorrect,
            };
        }

    }
}
