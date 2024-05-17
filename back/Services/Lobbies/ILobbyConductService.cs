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
        public Task<Result<QuestionData>> GetCurrentQuestion(string userId, string lobbyGuid);
        public Task<Result> RegisterAnswer(string userId, string lobbyGuid, string? answerGuid);
        public Task<Result<IEnumerable<AnswerData>>> GetRightAnswers(string userId, string lobbyGuid);
    }
}
