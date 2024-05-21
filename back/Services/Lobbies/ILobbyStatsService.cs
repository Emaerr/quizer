using Quizer.Models.Quizzes;

namespace Quizer.Services.Lobbies
{
    public interface ILobbyStatsService
    {
        public Stats GetQuizStats(string userId, string quizGuid);
    }
}
