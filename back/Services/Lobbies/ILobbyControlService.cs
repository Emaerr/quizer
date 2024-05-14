
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;
using Quizer.Models.User;

namespace Quizer.Services.Lobbies
{
    public interface ILobbyControlService 
    {
        public Task<Result<string>> CreateAsync(string masterId, string quizGuid, int maxParticipators);
        public Task<Result> JoinUserAsync(string lobbyGuid, string joiningUserId);
        public Task<Result> KickUserAsync(string userId, string lobbyGuid, string userToKickId);
        public Task<Result> StartLobbyAsync(string userId, string lobbyGuid);
        public Task<Result> ForceNextQuestionAsync(string userId, string lobbyGuid);
        public Task<Result> StopLobbyAsync(string userId, string lobbyGuid);
    }
}
