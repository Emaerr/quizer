using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizer.Models.User
{
    public class Participator
    {
        public Participator(string id) { 
            Id = id;
        }

        public Participator() { 
            Id = Guid.NewGuid().ToString();
        }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
    }
}
