using Quizer.Models.Lobbies;

namespace Quizer.Services.Lobbies
{
    public interface IParticipatorAnswerRepository
    {
        public IEnumerable<ParticipatorAnswer> GetAnswers();

        public ParticipatorAnswer? GetLobbyById(int id);

        public ParticipatorAnswer? GetLobbyByGuid(string guid);

        public void InsertLobby(ParticipatorAnswer lobby);

        public void DeleteLobby(int id);

        public void UpdateLobby(ParticipatorAnswer lobby);
        public void Save();
        public Task SaveAsync();
    }
}
