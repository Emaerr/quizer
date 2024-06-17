using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Quizer.Models.User;
using Quizer.Services.Lobbies;

namespace Quizer.Hubs
{
    public class LobbyHub(UserManager<ApplicationUser> userManager) : Hub<ILobbyClient>
    {
        public override async Task OnConnectedAsync()
        {
            ApplicationUser? user = await userManager.GetUserAsync(Context.User!);
            if (user == null)
            {
                return;
            }

            user.HubConnectionId = Context.ConnectionId;
            await userManager.UpdateAsync(user);
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            ApplicationUser? user = await userManager.GetUserAsync(Context.User!);
            if (user == null)
            {
                return;
            }

            user.HubConnectionId = null;
        }

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
}
