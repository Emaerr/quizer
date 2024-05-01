using Quizer.Exceptions.Models;
using Quizer.Models.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Xsl;

namespace Quizer.Models.Lobbies
{
    public class Lobby
    { 
        public Lobby()
        {
            Participators = new List<Participator>();
        }

        public int Id { get; set; }
        [ForeignKey("IdentityUser")]
        public int MasterId { get; set; }
        [ForeignKey("Quiz")]
        public int QuizId { get; set; }
        [Range(4, 1000)]
        public int MaxParticipators { get; set; }
        public virtual List<Participator> Participators { private get; set; }

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
