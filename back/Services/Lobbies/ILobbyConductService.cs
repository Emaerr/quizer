using FluentResults;
using Quizer.Models.Quizzes;
using Quizer.Services.Quizzes;

namespace Quizer.Services.Lobbies
{
    public enum LobbyStatus
    {
        Briefing,
        Question,
        Answering,
        Break,
        Results
    }

    public interface ILobbyConductService
    {
        delegate void LobbyStatusUpdateHandler(LobbyStatus lobbyStatus);

        Result SubsribeToLobbyStatusUpdateEvent(string lobbyGuid, LobbyStatusUpdateHandler handler);
        Result<LobbyStatus> GetLobbyStatus(string lobbyGuid);
        Result<Question> GetCurrentQuestion(string lobbyGuid);
        Task<Result> RegisterTestAnswer(string userId, string lobbyGuid, string? answerGuid);
        Task<Result> RegisterNumericalAnswer(string userId, string lobbyGuid, float? answer);
        Task<Result> RegisterTextAnswer(string userId, string lobbyGuid, string? answer);
    }
}
