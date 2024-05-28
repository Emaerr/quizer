using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Logging;
using Quizer.Exceptions.Models;
using Quizer.Models.Quizzes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Xsl;

namespace Quizer.Models.Lobbies
{
    enum LobbyTime
    {
        Question,
        Answering,
        Break,
        Results
    }


    [Table("Lobbies")]
    public class Lobby
    {
        private int _currentQuestion;
        private int _timeElapsedSinceLastAction; // here action is question finish and break finish
        private LobbyTime _lobbyTime;

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


        public bool IsQuestionTime()
        {
            return _lobbyTime == LobbyTime.Question;
        }

        public bool IsBreakTime()
        {
            return _lobbyTime == LobbyTime.Break;
        }

        public bool IsResultTime()
        {
            return _lobbyTime == LobbyTime.Results;
        }

        public bool IsAnsweringTime()
        {
            return _lobbyTime == LobbyTime.Answering;
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
            if (!IsStarted || IsResultTime())
            {
                return;
            }

            checked
            {
                try
                {
                    _timeElapsedSinceLastAction += (int)timeSpan.TotalMilliseconds;
                }
                catch (OverflowException e)
                {
                    throw new ModelException("Given time span is too high.", e);
                }
            }

            if (IsQuestionTime())
            {
                if (_timeElapsedSinceLastAction > Quiz.TimeLimit)
                {
                    _lobbyTime = LobbyTime.Answering;
                    _timeElapsedSinceLastAction = _timeElapsedSinceLastAction - Quiz.TimeLimit;
                }
                if (_currentQuestion == (Quiz.Questions.Count - 1))
                {
                    _lobbyTime = LobbyTime.Results;
                }
            } 
            else if (IsAnsweringTime())
            {
                if (_timeElapsedSinceLastAction > 1000)
                {
                    _lobbyTime = LobbyTime.Break;
                    _timeElapsedSinceLastAction = _timeElapsedSinceLastAction - 1000;
                }
            }
            else if (IsBreakTime())
            {
                if (_timeElapsedSinceLastAction > Quiz.BreakTime)
                {
                    NextQuestion();
                    _lobbyTime = LobbyTime.Question;
                    _timeElapsedSinceLastAction = _timeElapsedSinceLastAction - Quiz.BreakTime;
                }
            }

        }

        public void ResetTime()
        {
            _timeElapsedSinceLastAction = 0;
        }
    }
}
