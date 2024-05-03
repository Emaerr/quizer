using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizer.Models.User
{
    public class Participator
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        //[ForeignKey("Lobby")]
        //public int LobbyId {  get; set; }

        public virtual ApplicationUser? User { get; set; }
    }
}
