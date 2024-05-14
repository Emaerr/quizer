using FluentResults;
using Quizer.Services.Quizzes;

namespace Quizer.Services.Lobbies
{
    public interface ILobbyConductService
    {
        public Task<Result<QuestionData>> GetCurrentQuestion(string userId, string lobbyGuid);
    }
}
