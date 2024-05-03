using Microsoft.AspNetCore.Identity;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;
using Quizer.Models.User;

namespace Quizer.Services.Lobbies
{
    public interface ILobbyService
    {
        public Lobby Create(ApplicationUser master, Quiz quiz, int maxParticipators);


    }
}
