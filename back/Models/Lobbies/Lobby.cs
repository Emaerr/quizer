using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Logging;
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
        int _currentQuestion;

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
        public bool IsStarted { get; set; } = false;
        public virtual List<Participator> Participators { get; set; }
        public virtual Quiz? Quiz {  get; set; }

        public Question? GetCurrentQuestion()
        {
            if (Quiz != null) {
                return Quiz.GetQuestionByPosition(_currentQuestion);
            }
            return null;
        }

        public void NextQuestion()
        {
            _currentQuestion++;
        }

        public void PreviousQuestion() { 
            _currentQuestion--;
        }
    }
}
