using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Quizer.Data;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;
using Quizer.Models.User;

namespace Quizer.Services.Lobbies
{
    public class LobbyService : ILobbyService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public LobbyService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Lobby Create(ApplicationUser master, Quiz quiz, int maxParticipators)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();

            if (quiz.Id == null)
            {
                throw new ArgumentException("quiz.id is null");
            }

            Lobby lobby = new Lobby() {
                MasterId = master.Id,
                Quiz = quiz,
                MaxParticipators = maxParticipators,
            };
            lobbyRepository.InsertLobby(lobby);
            return lobby;
        }
    }
}
