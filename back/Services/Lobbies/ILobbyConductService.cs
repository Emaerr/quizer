using FluentResults;
using Quizer.Models.Quizzes;
using Quizer.Services.Quizzes;

namespace Quizer.Services.Lobbies
{
    public enum LobbyStatus
    {
        Briefing,
        Game,
        Results
    }

    public interface ILobbyConductService
    {
        public Result<LobbyStatus> GetLobbyStatus(string lobbyGuid);
        public Result<Question> GetCurrentQuestion(string lobbyGuid);
        public Task<Result> RegisterTestAnswer(string userId, string lobbyGuid, string? answerGuid);
        public Task<Result> RegisterNumericalAnswer(string userId, string lobbyGuid, float answer);
        public Task<Result> RegisterTextAnswer(string userId, string lobbyGuid, string answer);
        public Result<IEnumerable<Answer>> GetRightAnswers(string lobbyGuid);
    }
}
