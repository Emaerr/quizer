using Quizer.Models.Lobbies;

namespace Quizer.Services.Lobbies
{
    public interface ILobbyRepository
    {
        IEnumerable<Lobby> GetLobbies();
        Task<IEnumerable<Lobby>> GetLobbiesAsync();
        Lobby? GetLobbyById(int id);

        Lobby? GetLobbyByGuid(string guid);

        void InsertLobby(Lobby lobby);

        void DeleteLobby(int id);

        void UpdateLobby(Lobby lobby);
        void Save();
        Task SaveAsync();
    }
}
