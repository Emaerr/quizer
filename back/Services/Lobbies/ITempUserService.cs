using FluentResults;
using Quizer.Models.User;

namespace Quizer.Services.Lobbies
{

    /// <summary>
    /// Workaround
    /// </summary>
    public interface ITempUserService
    {
        Task<Result<ApplicationUser>> CreateTempUser(string displayName);

        /// <summary>
        /// Call before stopping lobby.
        /// </summary>
        /// <param name="lobbyGuid"></param>
        Task<Result> DeleteLobbyTempUsers(string lobbyGuid);
    }
}
