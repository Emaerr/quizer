using Quizer.Data;
using Quizer.Models.Lobbies;

namespace Quizer.Services.Lobbies
{
    public class LobbyService : ILobbyService
    {
        LobbyRepository _lobbies;

        public LobbyService(LobbyContext context)
        {
            _lobbies = new LobbyRepository(context);
        }

        public Lobby Create()
        {
            Lobby lobby = new Lobby();
            _lobbies.InsertLobby(lobby);
            return lobby;
        }
    }
}
