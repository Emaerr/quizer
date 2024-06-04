using FluentResults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Quizer.Exceptions.Services;
using Quizer.Models.Lobbies;
using Quizer.Models.User;
using Quizer.Services.Quizzes;

namespace Quizer.Services.Lobbies.impl
{
    public class TempUserService : ITempUserService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TempUserService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<Result<ApplicationUser>> CreateTempUser(string displayName)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string Id = Guid.NewGuid().ToString();
            ApplicationUser user = new ApplicationUser()
            {
                DisplayName = displayName,
                Id = Id,
                UserName = $"participator_{Id}",
                Email = $"{Id}@skibidistov.net",
                NormalizedUserName = $"participator_{Id}".ToUpper(),
                NormalizedEmail = $"{Id}@skibidistov.net".ToUpper(),
                EmailConfirmed = true,
                IsTemporal = true
            };
            await userManager.CreateAsync(user);
            await userManager.AddToRoleAsync(user, "Participator");

            return user;
        }

        public async Task<Result> DeleteLobbyTempUsers(string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);

            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError($"Lobby {lobbyGuid} not found"));
            }

            foreach (Participator participator in lobby.Participators)
            {
                if (participator.UserId == null)
                {
                    continue;
                }

                ApplicationUser? user = await userManager.FindByIdAsync(participator.UserId);
                if (user == null)
                {
                    continue;
                }

                if (user.IsTemporal)
                {
                    await userManager.DeleteAsync(user);
                }
            }

            return Result.Ok();
        }
    }
}
