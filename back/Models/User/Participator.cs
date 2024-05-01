using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizer.Models.User
{
    public class Participator : IdentityUser
    {
        public string? DisplayName { get; set; }

        [ForeignKey("Lobby")]
        public int LobbyId {  get; set; }
    }
}
