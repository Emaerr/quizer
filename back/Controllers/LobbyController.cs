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


namespace Quizer.Controllers
{
    [Route("[controller]")]
    public class LobbyController : Controller
    {
        private readonly ILogger<LobbyController> _logger;
        private readonly ILobbyControlService _lobbyService;
        private readonly IQrService _qrService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LobbyController(ILogger<LobbyController> logger, ILobbyControlService lobbyService, IQrService qrService, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _lobbyService = lobbyService;
            _qrService = qrService;
            _userManager = userManager;
        }

        [Authorize(Policy = "ParticipatorRights")]
        [HttpGet("Join/{lobbyGuid}")]
        public async Task<IActionResult> Join(string lobbyGuid)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result result = await _lobbyService.JoinUserAsync(lobbyGuid, user.Id);

            if (result.HasError<LobbyUnavailableError>())
            {
                return StatusCode(500);
            }
            if (result.HasError<MaxParticipatorsError>())
            { 
                return StatusCode(500);
            }
            if (result.HasError<LobbyNotFoundError>())
            {
                return NotFound();
            }
            if (result.HasError<UserNotFoundError>())
            {
                return StatusCode(500);
            }

            return RedirectToAction("Briefing", new {lobbyGuid});
        }

        [Authorize(Policy = "MemberRights")]
        [HttpGet("Manage/{lobbyGuid}")]
        public async Task<IActionResult> Manage()
        {
            return View();
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

        [Authorize(Policy = "MemberRights")]
        [HttpGet("Create")]
        public async Task<IActionResult> Create(string quizGuid, int maxParticipators)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            Result<string> result = await _lobbyService.CreateAsync(user.Id, quizGuid, (int)maxParticipators);

            if (result.IsFailed)
            {
                return StatusCode(500);
            }

            Uri uri = new Uri($"{Request.Scheme}://{Request.Host}/Lobby/Join/{result.Value}");

            _qrService.GenerateQrCode(result.Value, uri.ToString());

            return RedirectToAction("Manage", new { lobbyGuid = result.Value });
        }

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
    }
}
