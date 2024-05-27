using FluentResults;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;

namespace Quizer.Services.Lobbies
{
    public interface ILobbyStatsService
    { 
        public Task<Result<int>> GetUserPoints(string userId, string lobbyGuid);
        public Task<Result<IEnumerable<ParticipatorAnswer>>> GetUserAnswers(string userId, string lobbyGuid);
    };
}
