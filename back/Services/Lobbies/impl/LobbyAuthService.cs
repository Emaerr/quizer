using FluentResults;
using Microsoft.AspNetCore.Identity;
using Quizer.Exceptions.Services;
using Quizer.Models.Lobbies;
using Quizer.Models.User;
using Quizer.Services.Quizzes;
using Quizer.Services.Util;

namespace Quizer.Services.Lobbies.impl
{
    public class LobbyAuthService : ILobbyAuthService, IDisposable
    {
        private bool disposedValue;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITimeService _timeService;
        private readonly ILogger<LobbyService> _logger;

        public LobbyAuthService(IServiceScopeFactory scopeFactory, ITimeService timeService, ILogger<LobbyService> logger)
        {
            _scopeFactory = scopeFactory;
            _timeService = timeService;
            _logger = logger;
        }

        public async Task<Result<bool>> IsUserMaster(string userId, string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser? user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail(new UserNotFoundError("Invalid joining user id."));
            }

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            if (lobby.MasterId == user.Id)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<Result<bool>> IsUserParticipator(string userId, string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser? user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail(new UserNotFoundError("Invalid joining user id."));
            }

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            foreach (Participator participator in lobby.Participators)
            {
                if (participator.Id == userId)
                {
                    return true;
                }
            }

            return false;
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
        // ~LobbyAuthService()
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
