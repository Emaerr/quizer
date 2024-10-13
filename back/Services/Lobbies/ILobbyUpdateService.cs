using FluentResults;

namespace Quizer.Services.Lobbies
{
    public interface ILobbyUpdateService
    {
        delegate void LobbyStatusUpdateHandler(LobbyStatus lobbyStatus);

        Result SubscribeToLobbyStatusUpdateEvent(string lobbyGuid, LobbyStatusUpdateHandler handler);
    }
}
