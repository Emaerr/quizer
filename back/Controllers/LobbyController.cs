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


namespace Quizer.Controllers
{
    public class LobbyController : Controller
    {
        private readonly ILogger<LobbyController> _logger;
        private readonly ILobbyService _lobbyService;
        private readonly IQuizService _quizService;
        private readonly UserManager<IdentityUser> _userManager;

        public LobbyController(ILogger<LobbyController> logger, ILobbyService lobbyService, IQuizService quizService, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _lobbyService = lobbyService;
            _quizService = quizService;
            _userManager = userManager;
        }

        [Authorize(Policy = "MemberRights")]
        public async Task<IActionResult> Create([FromQuery(Name = "quizId")] int? quizId, [FromQuery(Name = "maxParticipators")] int? maxParticipators)
        {
            IdentityUser? user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }
            if (quizId == null || maxParticipators == null)
            {
                return BadRequest();
            }

            Quiz? quiz = _quizService.GetUserQuiz(user, (int)quizId);
            if (quiz !=  null) {
                Lobby lobby = _lobbyService.Create(user, quiz, (int)maxParticipators);
                return View(lobby);
            } else
            {
                return UnprocessableEntity();
            }

        }

        [Authorize(Policy = "ParticipatorRights")]
        public IActionResult Join(int? id)
        {
            return View();
        }
    }
}
