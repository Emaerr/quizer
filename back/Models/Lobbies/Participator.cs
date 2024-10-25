using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizer.Models.Lobbies
{
    public class Participator
    {
        public Participator(string userId)
        {
            UserId = userId;
        }

        public Participator()
        {
            Id = Guid.NewGuid().ToString();
        }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [ForeignKey("ApplicationUser")]
        public string? UserId { get; set; } 
        public List<ParticipatorAnswer> Answers { get; set; } = [];
        public int Points { get; set; }
    }
}
