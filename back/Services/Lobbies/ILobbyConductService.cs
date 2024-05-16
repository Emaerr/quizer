using FluentResults;
using Quizer.Services.Quizzes;

namespace Quizer.Services.Lobbies
{
    public interface ILobbyConductService
    {
        public Task<Result<QuestionData>> GetCurrentQuestion(string userId, string lobbyGuid);
        public Task<Result> RegisterAnswer(string userId, string lobbyGuid, string? answerGuid);
        public Task<Result<IEnumerable<AnswerData>>> GetRightAnswers(string userId, string lobbyGuid);
    }
}
