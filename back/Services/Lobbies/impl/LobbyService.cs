using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Quizer.Data;
using Quizer.Exceptions.Models;
using Quizer.Exceptions.Services;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;
using Quizer.Models.User;
using Quizer.Services.Quizzes;

namespace Quizer.Services.Lobbies.impl
{
    public class LobbyService : ILobbyService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IQuizControlService _quizControlService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LobbyService(IServiceScopeFactory scopeFactory, IQuizControlService quizControlService, UserManager<ApplicationUser> userManager)
        {
            _scopeFactory = scopeFactory;
            _quizControlService = quizControlService;
            _userManager = userManager;
        }

        public async Task<Result<string>> CreateAsync(string masterId, string quizGuid, int maxParticipators)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();

            Lobby lobby = new Lobby()
            {
                MasterId = masterId,
                Quiz = quizRepository.GetQuizByGuid(quizGuid),
                MaxParticipators = maxParticipators,
            };
            lobbyRepository.InsertLobby(lobby);
            await lobbyRepository.SaveAsync();

            if (lobby.Guid != null) {
                return Result.Ok(lobby.Guid);
            } else
            {
                return Result.Fail("Impossible happened. Lobby GUID is null. (check lobby constructor)");
            }

        }

        public async Task<Result> ForceNextQuestionAsync(string userId, string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();

            ApplicationUser? user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail(new UserNotFoundError("Invalid joining user id."));
            }

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            _quizControlService.ForceNextQuestion(lobbyGuid);
            return Result.Ok();            
        }

        public async Task<Result> JoinUserAsync(string lobbyGuid, string joiningUserId)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();

            ApplicationUser? user = await _userManager.FindByIdAsync(joiningUserId);
            if (user == null)
            {
                return Result.Fail(new UserNotFoundError("Invalid joining user id."));
            }

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            Participator participator = new Participator(joiningUserId);

            if (lobby.IsStarted)
            {
                return Result.Fail(new LobbyUnavailableError("The game has already started."));
            }

            if (lobby.Participators.Count() <= lobby.MaxParticipators)
            {
                lobby.Participators.Add(participator);
            }
            else
            {
                return Result.Fail(new MaxParticipatorsError("No free player slot in the lobby."));
            }

            return Result.Ok();
        }

        public async Task<Result> KickUserAsync(string userId, string lobbyGuid, string userToKickId)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();

            ApplicationUser? user = await _userManager.FindByIdAsync(userToKickId);
            if (user == null)
            {
                return Result.Fail(new UserNotFoundError("Invalid user to kick id."));
            }

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("(Invalid lobby GUID."));
            }

            foreach (Participator participator in lobby.Participators)
            {
                if (participator.Id == userToKickId)
                {
                    lobby.Participators.Remove(participator);
                }
            }

            return Result.Ok();
        }

        public async Task<Result> StartAsync(string userId, string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();

            ApplicationUser? user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail(new UserNotFoundError("Invalid user id."));
            }

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            lobby.IsStarted = true;

            return Result.Ok();
        }

        public async Task<Result> StopAsync(string userId, string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();

            ApplicationUser? user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail(new UserNotFoundError("Invalid user id."));
            }

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            lobby.IsStarted = false;
            lobbyRepository.DeleteLobby(lobby.Id);
            await lobbyRepository.SaveAsync();

            return Result.Ok();
        }
    }
}
