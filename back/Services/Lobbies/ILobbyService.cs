
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;
using Quizer.Models.User;

namespace Quizer.Services.Lobbies
{
    public interface ILobbyService 
    {
        public Task<Result<string>> CreateAsync(string masterId, string quizGuid, int maxParticipators);
        public Task<Result> JoinUserAsync(string lobbyGuid, string joiningUserId);
        public Task<Result> KickUserAsync(string userId, string lobbyGuid, string userToKickId);
        public Task<Result> StartAsync(string userId, string lobbyGuid);
        public Task<Result> ForceNextQuestionAsync(string userId, string lobbyGuid);
        public Task<Result> StopAsync(string userId, string lobbyGuid);
    }
}
