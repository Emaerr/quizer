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
        private int _currentQuestion;
        private int _timeElapsedSinceLastAction; // here action is question finish and break finish
        bool _isBreakTime;

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
        public int Pin { get; set; }
        public virtual List<Participator> Participators { get; set; }
        public virtual Quiz? Quiz {  get; set; }


        public bool IsBreakTime()
        {
            return _isBreakTime;
        }

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

        public void Update(TimeSpan timeSpan)
        {
            if (!IsStarted)
            {
                return;
            }

            checked
            {
                try
                {
                    _timeElapsedSinceLastAction = (int)timeSpan.TotalMilliseconds;
                }
                catch (OverflowException e)
                {
                    throw new ModelException("Given time span is too high.", e);
                }
            }

            if (!_isBreakTime)
            {
                if ((float)_timeElapsedSinceLastAction / 1000.0f >= Quiz.TimeLimit)
                {
                    _isBreakTime = true;
                    _timeElapsedSinceLastAction = 0;
                }
            } else
            {
                if ((float)_timeElapsedSinceLastAction / 1000.0f >= Quiz.BreakTime)
                {
                    NextQuestion();
                    _isBreakTime = false;
                    _timeElapsedSinceLastAction = 0;
                }
            }

        }

        public void ResetTime()
        {
            _timeElapsedSinceLastAction = 0;
        }
    }
}
