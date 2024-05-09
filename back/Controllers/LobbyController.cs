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


namespace Quizer.Controllers
{
    public class LobbyController : Controller
    {
        private readonly ILogger<LobbyController> _logger;
        private readonly ILobbyService _lobbyService;
        private readonly IQuizDataRepository _quizService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LobbyController(ILogger<LobbyController> logger, ILobbyService lobbyService, IQuizDataRepository quizService, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _lobbyService = lobbyService;
            _quizService = quizService;
            _userManager = userManager;
        }

        [Authorize(Policy = "MemberRights")]
        public async Task<IActionResult> Create([FromQuery(Name = "quizId")] string? quizGuid, [FromQuery(Name = "maxParticipators")] int? maxParticipators)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }
            if (quizGuid == null || maxParticipators == null)
            {
                return BadRequest();
            }

            //Quiz? quiz = _quizService.GetUserQuizData(user.Id, quizGuid);
            //if (quiz !=  null) {
            //    Lobby lobby = _lobbyService.Create(user, quiz, (int)maxParticipators);
            //    return View(lobby);
            //} else
            //{
            //    return UnprocessableEntity();
            //}

            return View();
        }

        [Authorize(Policy = "ParticipatorRights")]
        public IActionResult Join(int id)
        {
            return View();
        }
    }
}
