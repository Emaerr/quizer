using FluentResults;

namespace Quizer.Services.Lobbies
{
    public interface ILobbyAuthService
    {
        public Task<Result<bool>> IsUserMaster(string userId, string lobbyGuid);
        public Task<Result<bool>> IsUserParticipator(string userId, string lobbyGuid);
    }
}
