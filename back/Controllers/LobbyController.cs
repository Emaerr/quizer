﻿using Microsoft.AspNetCore.Authorization;
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


namespace Quizer.Controllers
{
    [Route("[controller]")]
    public class LobbyController : Controller
    {
        private readonly ILogger<LobbyController> _logger;
        private readonly ILobbyControlService _lobbyControlService;
        private readonly ILobbyAuthService _lobbyAuthService;
        private readonly ILobbyConductService _lobbyConductService;
        private readonly IQrService _qrService;
        private readonly ITempUserService _tempUserService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LobbyController(
            ILogger<LobbyController> logger, ILobbyControlService lobbyControlService,
            ILobbyConductService lobbyConductService, ILobbyAuthService lobbyAuthService,
            IQrService qrService, ITempUserService tempUserService, UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser>  signInManager)
        {
            _logger = logger;
            _lobbyControlService = lobbyControlService;
            _lobbyConductService = lobbyConductService;
            _lobbyAuthService = lobbyAuthService;
            _qrService = qrService;
            _tempUserService = tempUserService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet("Join/{lobbyGuid}")]
        public async Task<IActionResult> Join(string lobbyGuid, string? error = null)
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
        /// <returns>
        /// If all ok redirects to Briefing.
        /// If lobby is unavailable returns Unavailable view.
        /// If no free slot (lobby reached max participators count) returns NoFreeSlot view.
        /// </returns>
        [HttpPost("JoinConfirm/{lobbyGuid}")]
        public async Task<IActionResult> JoinConfirm(string lobbyGuid, [FromForm] string displayName)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Result<ApplicationUser> resultUser = await _tempUserService.CreateTempUser(displayName);

                if (resultUser.IsSuccess) {
                    user = resultUser.Value;
                    await _signInManager.SignInAsync(user, new AuthenticationProperties { IsPersistent = true, AllowRefresh = true });
                }
                else
                {
                    return StatusCode(500);
                }
            }

            Result result = await _lobbyControlService.JoinUserAsync(lobbyGuid, user.Id);

            if (result.IsFailed && user.IsTemporal)
            {
                await _userManager.DeleteAsync(user);
            }

            if (result.HasError<LobbyUnavailableError>())
            {
                return View("Unavailable");
            }
            if (result.HasError<MaxParticipatorsError>())
            {
                return View("NoFreeSlot"); ;
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

            return RedirectToAction("Briefing", new { lobbyGuid });
        }

        [Authorize(Policy = "ParticipatorRights")]
        [HttpPost("Leave/{lobbyGuid}")]
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
        /// <returns>Returns GameMaster or GameParticipator view with current question view model</returns>
        [Authorize(Policy = "ParticipatorRights")]
        [HttpGet("Game/{lobbyGuid}")]
        public async Task<IActionResult> Game(string lobbyGuid)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result<bool> resultParticipatorCheck = await _lobbyAuthService.IsUserParticipator(user.Id, lobbyGuid);
            Result<bool> resultMasterCheck = await _lobbyAuthService.IsUserMaster(user.Id, lobbyGuid);

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

            Result<Question> result = _lobbyConductService.GetCurrentQuestion(lobbyGuid);

            if (result.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }
            if (result.HasError<QuizNotFoundError>())
            {
                return NotFound();
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

        [Authorize(Policy = "ParticipatorRights")]
        [HttpPost("RegisterAnswer/{lobbyGuid}")]
        public async Task<IActionResult> RegisterAnswer(string lobbyGuid, string? answerGuid)
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

            Result<Question> questionResult = _lobbyConductService.GetCurrentQuestion(lobbyGuid);

            Result result = await _lobbyConductService.RegisterTestAnswer(user.Id, lobbyGuid, answerGuid);

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
        /// <returns>Question view model, guid of user answer and is this answer correct</returns>
        /// <example><code>@model (QuestionViewModel question, string userAnswerGuid, bool isUserAnswerCorrect)</code></example>
        [Authorize(Policy = "ParticipatorRights")]
        [HttpGet("Result/{lobbyGuid}")]
        public async Task<IActionResult> Result(string lobbyGuid)
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

            Result<Question> questionResult = _lobbyConductService.GetCurrentQuestion(lobbyGuid);

            Result<IEnumerable<Answer>> rightAnswers = _lobbyConductService.GetRightAnswers(lobbyGuid);

            if (rightAnswers.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }
            if (rightAnswers.HasError<QuizNotFoundError>())
            {
                return NotFound();
            }

            bool isAnswerCorrect = false;

            foreach (Answer answerData in questionResult.Value.Answers)
            {
                if (answerData.IsCorrect)
                {
                    isAnswerCorrect = true;
                }
            }

            QuestionViewModel viewModel = GetQuestionViewModel(questionResult.Value);

            var model = (viewModel, "answerGuid", isAnswerCorrect);

            return View(model);
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
        public async Task<IActionResult> Manage(string lobbyGuid)
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
        [HttpPost("Start/{lobbyGuid}")]
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
        [HttpPost("Stop/{lobbyGuid}")]
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

            Result resultUsers = await _tempUserService.DeleteLobbyTempUsers(lobbyGuid);
            Result result = await _lobbyControlService.StopLobbyAsync(lobbyGuid);

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


        [Authorize(Policy = "MemberRights")]
        [HttpPost("ForceNextQuestion/{lobbyGuid}")]
        public async Task<IActionResult> ForceNextQuestion(string lobbyGuid)
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

            _lobbyControlService.ForceNextQuestionAsync(lobbyGuid); // ignore result, because the only error it can return is LobbyNotFound

            return Ok();
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

    }
}
