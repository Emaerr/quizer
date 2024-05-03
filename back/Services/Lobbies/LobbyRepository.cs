using Microsoft.EntityFrameworkCore;
using Quizer.Data;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;

namespace Quizer.Services.Lobbies
{
    public class LobbyRepository : ILobbyRepository
    {
        private AppDbContext _context;

        public LobbyRepository(AppDbContext context)
        {
            _context = context;
        }


        public IEnumerable<Lobby> GetLobbies()
        {
            return _context.Lobbies.ToList();
        }

        public Lobby? GetLobbyById(int id)
        {
            return _context.Lobbies.Find(id);
        }

        public void InsertLobby(Lobby lobby)
        {
            _context.Lobbies.Add(lobby);
        }

        public void DeleteLobby(int id)
        {
            Lobby? lobby = _context.Lobbies.Find(id);
            if (lobby != null)
            {
                _context.Lobbies.Remove(lobby);
            }
        }

        public void UpdateLobby(Lobby lobby)
        {
            _context.Entry(lobby).State = EntityState.Modified;
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public Lobby? GetLobbyByGuid(string guid)
        {
            var lobbies = from l in _context.Lobbies where l.Guid == guid select l;
            return lobbies.FirstOrDefault();
        }
    }
}
