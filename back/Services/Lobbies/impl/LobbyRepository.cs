using Microsoft.EntityFrameworkCore;
using Quizer.Data;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;

namespace Quizer.Services.Lobbies.impl
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
            var lobbies = _context.Lobbies;
            return lobbies;
        }

        public async Task<IEnumerable<Lobby>> GetLobbiesAsync()
        {
            var lobbies = await _context.Lobbies.ToListAsync();
            return lobbies;
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
            _context.Attach(lobby);
            _context.Update(lobby);
        }

        public void Save()
        {
            var saved = false;
            while (!saved)
            {
                try
                {
                    // Attempt to save changes to the database
                    _context.SaveChanges();
                    saved = true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        if (entry.Entity is Lobby)
                        {
                            var proposedValues = entry.CurrentValues;
                            var databaseValues = entry.GetDatabaseValues();

                            foreach (var property in proposedValues.Properties)
                            {
                                var proposedValue = proposedValues[property];
                                var databaseValue = databaseValues[property];
                                
                                // Access Denied Lobby start bug case
                                if (property.Name == "IsStarted" && proposedValue != databaseValue)
                                {
                                    proposedValues[property] = databaseValue;
                                }
                            }

                            // Refresh original values to bypass next concurrency check
                            entry.OriginalValues.SetValues(databaseValues);
                        }
                        else
                        {
                            throw new NotSupportedException(
                                "Don't know how to handle concurrency conflicts for "
                                + entry.Metadata.Name);
                        }
                    }
                }
            }
        }

        public async Task SaveAsync()
        {
            var saved = false;
            while (!saved)
            {
                try
                {
                    // Attempt to save changes to the database
                    await _context.SaveChangesAsync();
                    saved = true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        if (entry.Entity is Lobby)
                        {
                            var proposedValues = entry.CurrentValues;
                            var databaseValues = entry.GetDatabaseValues();

                            foreach (var property in proposedValues.Properties)
                            {
                                var proposedValue = proposedValues[property];
                                var databaseValue = databaseValues[property];

                                // Access Denied Lobby start bug case
                                if (property.Name == "IsStarted" && proposedValue != databaseValue)
                                { 
                                    proposedValues[property] = databaseValue;
                                }
                            }

                            // Refresh original values to bypass next concurrency check
                            entry.OriginalValues.SetValues(databaseValues);
                        }
                        else
                        {
                            throw new NotSupportedException(
                                "Don't know how to handle concurrency conflicts for "
                                + entry.Metadata.Name);
                        }
                    }
                }
            }
        }

        public Lobby? GetLobbyByGuid(string guid)
        {
            var lobbies = from l in _context.Lobbies where l.Guid == guid select l;
            return lobbies.FirstOrDefault();
        }
    }
}
