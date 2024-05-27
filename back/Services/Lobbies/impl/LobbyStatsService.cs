using FluentResults;
using Microsoft.AspNetCore.Identity;
using Quizer.Exceptions.Services;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;
using Quizer.Models.User;
using Quizer.Services.Quizzes;
using Quizer.Services.Util;
using System.Collections.Immutable;

namespace Quizer.Services.Lobbies.impl
{
    public class LobbyStatsService : ILobbyStatsService, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LobbyAuthService> _logger;
        private bool disposedValue;

        public LobbyStatsService(IServiceScopeFactory scopeFactory, ILogger<LobbyAuthService> logger) { 
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<Result<int>> GetUserPoints(string lobbyGuid, string userId)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            ApplicationUser? user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail(new UserNotFoundError("Invalid joining user id."));
            }

            int? points = null;

            foreach (Participator participator in lobby.Participators)
            {
                if (participator.Id == userId) { 
                    points = participator.Points;
                }
            }

            if (points == null)
            {
                return Result.Fail(new LobbyAccessDeniedError("User is not in the lobby."));
            }

            return Result.Ok((int)points);
        }

        public async Task<Result<IEnumerable<ParticipatorAnswer>>> GetUserAnswers(string userId, string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            ApplicationUser? user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail(new UserNotFoundError("Invalid joining user id."));
            }

            Participator? participator = null;

            foreach (Participator p in lobby.Participators)
            {
                if (p.Id == userId)
                {
                    participator = p;
                }
            }

            if (participator == null)
            {
                return Result.Fail(new LobbyAccessDeniedError("User is not in the lobby."));
            }

            return Result.Ok((IEnumerable<ParticipatorAnswer>)participator.Answers);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~LobbyStatsService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
