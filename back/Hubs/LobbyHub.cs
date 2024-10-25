using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Quizer.Models.User;
using Quizer.Services.Lobbies;
using System.Runtime.InteropServices;

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

            await Groups.AddToGroupAsync(Context.ConnectionId, "user_" + user.Id);
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            ApplicationUser? user = await userManager.GetUserAsync(Context.User!);
            if (user == null)
            {
                return;
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "user_" + user.Id);
        }

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
}
