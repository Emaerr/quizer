using Quizer.Models.Lobbies;
using Quizer.Models.User;

namespace Quizer.Services.Lobbies
{
    public interface IParticipatorRepository
    {
        public IEnumerable<Participator> GetParticipators();
        public Participator? GetParticipatorById(string id);
        public void InsertParticipator(Participator participator);
        public void DeleteParticipator(string id);
        public void UpdateParticipator(Participator participator);
        public void Save();
        public Task SaveAsync();
    }
}
