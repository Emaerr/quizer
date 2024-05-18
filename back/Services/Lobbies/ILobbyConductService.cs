using FluentResults;
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
        public Result<QuestionData> GetCurrentQuestion(string lobbyGuid);
        public Task<Result> RegisterTestAnswer(string userId, string lobbyGuid, string? answerGuid);
        public Task<Result> RegisterNumericalAnswer(string userId, string lobbyGuid, int answer);
        public Task<Result> RegisterTextAnswer(string userId, string lobbyGuid, string answer);
        public Result<IEnumerable<AnswerData>> GetRightAnswers(string lobbyGuid);
    }
}
