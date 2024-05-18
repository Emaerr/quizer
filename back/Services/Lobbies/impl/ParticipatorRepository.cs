using Microsoft.EntityFrameworkCore;
using Quizer.Data;
using Quizer.Models.Lobbies;
using System;

namespace Quizer.Services.Lobbies.impl
{
    public class ParticipatorRepository : IParticipatorRepository
    {
        private AppDbContext _context;

        public ParticipatorRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Participator> GetParticipators() {
            return _context.Participators.ToList();
        }

        public Participator? GetParticipatorById(string id) { 
            return _context.Participators.Find(id); ;
        }

        public void InsertParticipator(Participator participator) {
            _context.Participators.Add(participator);
        }

        public void DeleteParticipator(string id) {
            Participator? participator = _context.Participators.Find(id);
            if (participator != null)
            {
                _context.Participators.Remove(participator);
            }
        }

        public void UpdateParticipator(Participator participator) {
            _context.Entry(participator).State = EntityState.Modified;
        }

        public void Save() { _context.SaveChanges(); }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
