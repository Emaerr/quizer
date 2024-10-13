using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Logging;
using Quizer.Exceptions.Models;
using Quizer.Models.Quizzes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Xsl;

namespace Quizer.Models.Lobbies
{
    public enum LobbyStage
    {
        Briefing,
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

        public delegate void LobbyStageChange(LobbyStage stage);
        public event LobbyStageChange OnLobbyStageChange;

        public Lobby()
        {
            Stage = LobbyStage.Briefing;
            Participators = new List<Participator>();
        }

        public Lobby(LobbyStage lobbyStage)
        {
            Stage = lobbyStage;
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
        public LobbyStage Stage { get; set; } = LobbyStage.Question; // probably onLobbyStageChange should be here in the setter
        public int CurrentQuestionPosition { get; set; }
        public int Pin { get; set; }
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
            _timeElapsedSinceLastAction = 0;
            Stage = LobbyStage.Question;
            _currentQuestion++;
            //if (OnLobbyStageChange != null)
            //{
            //    OnLobbyStageChange(LobbyStage.Question);
            //}
        }

        /// <summary>
        /// DEPRECATED.
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <exception cref="ModelException"></exception>
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
                    Stage = LobbyStage.Answering;
                    _timeElapsedSinceLastAction = _timeElapsedSinceLastAction - Quiz.TimeLimit;
                    if (OnLobbyStageChange != null)
                    {
                        OnLobbyStageChange(LobbyStage.Answering);
                    }
                }
                if (_currentQuestion == (Quiz.Questions.Count - 1))
                {
                    Stage = LobbyStage.Results;
                    if (OnLobbyStageChange != null)
                    {
                        OnLobbyStageChange(LobbyStage.Results);
                    }
                }
            } 
            else if (IsAnsweringTime())
            {
                if (_timeElapsedSinceLastAction > 1000)
                {
                    Stage = LobbyStage.Break;
                    _timeElapsedSinceLastAction = _timeElapsedSinceLastAction - 1000;
                    if (OnLobbyStageChange != null)
                    {
                        OnLobbyStageChange(LobbyStage.Break);
                    }
                }
            }
            else if (IsBreakTime())
            {
                if (_timeElapsedSinceLastAction > Quiz.BreakTime)
                {
                    NextQuestion();
                    _timeElapsedSinceLastAction = _timeElapsedSinceLastAction - Quiz.BreakTime;
                }
            }

        }

        public void Start()
        {
            IsStarted = true;
            //if (OnLobbyStageChange != null)
            //{
            //    OnLobbyStageChange(LobbyStage.Question);
            //}
        }

        public void ResetTime()
        {
            _timeElapsedSinceLastAction = 0;
        }

        public bool IsQuestionTime()
        {
            return Stage == LobbyStage.Question;
        }

        public bool IsBreakTime()
        {
            return Stage == LobbyStage.Break;
        }

        public bool IsResultTime()
        {
            return Stage == LobbyStage.Results;
        }

        public bool IsAnsweringTime()
        {
            return Stage == LobbyStage.Answering;
        }

        public bool IsBriefingTime()
        {
            return Stage == LobbyStage.Briefing;
        }
    }
}
