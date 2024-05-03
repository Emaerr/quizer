using Microsoft.AspNetCore.Identity;
using Quizer.Exceptions.Models;
using Quizer.Models.Quizzes;
using Quizer.Models.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Xsl;

namespace Quizer.Models.Lobbies
{
    [Table("Lobbies")]
    public class Lobby
    { 
        public Lobby()
        {
            Participators = new List<Participator>();
        }
        public int Id { get; set; }
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? Guid { get; set; } = System.Guid.NewGuid().ToString();
        [ForeignKey("ApplicationUser")]
        public string MasterId { get; set; }
        [Range(4, 1000)]
        public int MaxParticipators { get; set; }
        public virtual List<Participator> Participators { private get; set; }
        public virtual Quiz? Quiz {  get; set; }

        private bool _isStarted = false;

        public void Start()
        {
            _isStarted = true;
        }

        public void AddParticipator(Participator participator)
        {   
            if (_isStarted)
            {
                throw new LobbyUnavailable();
            }

            if (Participators.Count() <= MaxParticipators)
            {
                Participators.Add(participator);
            } else
            {
                throw new MaxParticipatorsException();
            }
        }

        public void RemoveParticipator(Participator participator) {
            Participators.Remove(participator);
        }

    }
}
