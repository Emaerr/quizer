using Microsoft.AspNetCore.Identity;
using Quizer.Data;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;

namespace Quizer.Services.Lobbies
{
    public class LobbyService : ILobbyService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public LobbyService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Lobby Create(IdentityUser master, Quiz quiz, int maxParticipators)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();

            Lobby lobby = new Lobby() {
                MasterId = master.Id,
                QuizId = quiz.Id,
                MaxParticipators = maxParticipators,
            };
            lobbyRepository.InsertLobby(lobby);
            return lobby;
        }
    }
}
