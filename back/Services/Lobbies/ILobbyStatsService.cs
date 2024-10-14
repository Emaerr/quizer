using FluentResults;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;

namespace Quizer.Services.Lobbies
{
    public interface ILobbyStatsService
    { 
        public Task<Result<int>> GetUserPoints(string lobbyGuid, string userId);
        public Task<Result<IEnumerable<ParticipatorAnswer>>> GetUserAnswers(string lobbyGuid, string userId);
    };
}
