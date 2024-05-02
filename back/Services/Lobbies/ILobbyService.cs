using Microsoft.AspNetCore.Identity;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;

namespace Quizer.Services.Lobbies
{
    public interface ILobbyService
    {
        public Lobby Create(IdentityUser master, Quiz quiz, int maxParticipators);


    }
}
