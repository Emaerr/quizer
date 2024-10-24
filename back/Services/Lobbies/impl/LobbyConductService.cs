using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Quizer.Exceptions.Services;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;
using Quizer.Models.User;
using Quizer.Services.Quizzes;
using Quizer.Services.Util;
using static Quizer.Services.Lobbies.ILobbyConductService;

namespace Quizer.Services.Lobbies.impl
{
    public class LobbyConductService : ILobbyConductService, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LobbyConductService> _logger;
        private bool disposedValue;

        public LobbyConductService(IServiceScopeFactory scopeFactory, ILogger<LobbyConductService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Result<LobbyStatus> GetLobbyStatus(string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            if (lobby.Stage == LobbyStage.Results)
            {
                return LobbyStatus.Result;
            }
            else if (lobby.Stage == LobbyStage.Question)
            {
                return LobbyStatus.Question;
            }
            else if (lobby.Stage == LobbyStage.Answering)
            {
                return LobbyStatus.Answering;
            }
            else if (lobby.Stage == LobbyStage.Break)
            {
                return LobbyStatus.Break;
            }

            throw new Exception("Something is wrong with else if in LobbyConductService GetLobbyStatus");
        }

        public Result<Question> GetCurrentQuestion(string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            if (!lobby.IsStarted)
            {
                return Result.Fail(new LobbyUnavailableError("Lobby isn't started."));
            }
            if (lobby.Stage == LobbyStage.Results)
            {
                return Result.Fail(new LobbyUnavailableError("Game has already finished."));
            }

            if (lobby.Quiz == null)
            {
                return Result.Fail(new QuizNotFoundError("Lobby doesn't have quiz."));
            }

            Question? question = lobby.GetCurrentQuestion();
            if (question == null)
            {
                return Result.Fail(new QuestionNotFoundError("The question is null."));
            }

            return Result.Ok(question);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Total count of questions in current quiz.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public Result<int> GetQuestionCount(string lobbyGuid)
		{
			IServiceScope scope = _scopeFactory.CreateScope();
			ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();

			Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
			if (lobby == null)
			{
				return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
			}

			if (!lobby.IsStarted)
			{
				return Result.Fail(new LobbyUnavailableError("Lobby has't started yet."));
			}
			if (lobby.Stage == LobbyStage.Results)
			{
				return Result.Fail(new LobbyUnavailableError("Game has already finished."));
			}

            if (lobby.Quiz == null)
            {
				return Result.Fail(new QuizNotFoundError("Lobby doesn't have quiz."));
			}

            return Result.Ok(lobby.Quiz.Questions.Count);
		}


        public Result<int> GetTimeLimit(string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            Quiz? quiz = lobby.Quiz;

            if (quiz == null)
            {
                return Result.Fail(new QuizNotFoundError("Lobby doesn't have quiz."));
            }

            return Result.Ok(quiz.TimeLimit);
        }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="lobbyGuid"></param>
		/// <param name="answerGuid">Should be a valid question answer GUID if the user answered, and null if the user didn't answer</param>
		/// <returns></returns>
		public async Task<Result> RegisterTestAnswer(string userId, string lobbyGuid, string? answerGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            IParticipatorRepository participatorRepository = scope.ServiceProvider.GetRequiredService<IParticipatorRepository>();

            Result<Lobby> lobbyResult = await GetLobbyForAnswerRegistration(userId, lobbyGuid, answerGuid, QuestionType.Test);
            if (lobbyResult.IsFailed)
            {
                return Result.Fail(lobbyResult.Errors.First());
            }

            Lobby lobby = lobbyResult.Value;

            Participator? participator = GetLobbyParticipator(lobby, userId);
            if (participator == null)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register test answer {answerGuid} because user {userId} doesn't participate in lobby {lobbyGuid}", answerGuid, userId, lobbyGuid);
                return Result.Fail(new LobbyAccessDeniedError($"User {userId} is not part of the lobby {lobbyGuid}"));
            }

            Question? currentQuestion = lobby.GetCurrentQuestion();
            if (currentQuestion == null)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register test answer {answerGuid} for user {userId} in {lobbyGuid} bacause currentQuestion not found (is null)", answerGuid, userId, lobbyGuid);
                return Result.Fail(new QuizNotFoundError("The question is null. Possible reasion is that quiz in lobby is null."));
            }

            if (currentQuestion.Type != QuestionType.Test)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register test answer {answerGuid} for user {userId} in lobby {lobbyGuid} because current question type is not test", answerGuid, userId, lobbyGuid);
                return Result.Fail(new InvalidAnswerFormatError("Invalid answer format."));
            }

            IEnumerable<ParticipatorAnswer> currentQuestionParticipatorAnswers = from a in participator.Answers where a.Question.Guid == currentQuestion.Guid select a;
            if (currentQuestionParticipatorAnswers.Any())
            {
                return Result.Fail(new QuestionAlreadyAnsweredError("Question has already been answered."));
            }

            ParticipatorAnswer? participatorAnswer = null;

            if (answerGuid == null)
            {
                participatorAnswer = new ParticipatorAnswer()
                {
                    TestAnswer = null,
                    Question = currentQuestion
                };
            }

            IEnumerable<Answer> answers = from a in currentQuestion.Answers where a.Guid == answerGuid select a;
            if (!answers.IsNullOrEmpty())
            {
                Answer answer = answers.First();
                participatorAnswer = new ParticipatorAnswer()
                {
                    TestAnswer = answer,
                    Question = currentQuestion
                };
                if (answer.IsCorrect)
                {
                    participator.Points++;
                }
            }

            if (participatorAnswer == null)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register test answer {answerGuid} for user {userId} in lobby {lobbyGuid} because the answer not found (answer GUID is not valid)", answerGuid, userId, lobbyGuid);
                return Result.Fail(new InvalidAnswerGuidError("Invalid answer GUID"));
            }

            participator.Answers.Add(participatorAnswer);
            participatorRepository.UpdateParticipator(participator);
            await participatorRepository.SaveAsync();

            _logger.LogInformation(ServiceLogEvents.AnswerRegistered, "Succesfully registered test answer {answerGuid} for user {userId} in lobby {lobbyGuid}", answerGuid, userId, lobbyGuid);

            return Result.Ok();
        }

        public async Task<Result> RegisterNumericalAnswer(string userId, string lobbyGuid, float? answer)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            IParticipatorRepository participatorRepository = scope.ServiceProvider.GetRequiredService<IParticipatorRepository>();

            Result<Lobby> lobbyResult = await GetLobbyForAnswerRegistration(userId, lobbyGuid, answer.ToString(), QuestionType.NumberEntry);
            if (lobbyResult.IsFailed)
            {
                return Result.Fail(lobbyResult.Errors.First());
            }

            Lobby lobby = lobbyResult.Value;

            Participator? participator = GetLobbyParticipator(lobby, userId);
            if (participator == null)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register numerical answer {answer} because user {userId} doesn't participate in lobby {lobbyGuid}", answer, userId, lobbyGuid);
                return Result.Fail(new LobbyAccessDeniedError($"User {userId} is not part of the lobby {lobbyGuid}"));
            }

            Question? currentQuestion = lobby.GetCurrentQuestion();
            if (currentQuestion == null)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register numerical answer {answer} for user {userId} in {lobbyGuid} bacause currentQuestion not found (is null)", answer, userId, lobbyGuid);
                return Result.Fail(new QuizNotFoundError("The question is null. Possible reasion is that quiz in lobby is null."));
            }

            if (currentQuestion.Type != QuestionType.NumberEntry)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register numerical answer {answer} for user {userId} in lobby {lobbyGuid} because current question type is not NumberEntry", answer, userId, lobbyGuid);
                return Result.Fail(new InvalidAnswerFormatError("Invalid answer format."));
            }

            IEnumerable<ParticipatorAnswer> currentQuestionParticipatorAnswers = from a in participator.Answers where a.Question.Guid == currentQuestion.Guid select a;
            if (currentQuestionParticipatorAnswers.Any())
            {
                return Result.Fail(new QuestionAlreadyAnsweredError("Question has already been answered."));
            }

            ParticipatorAnswer? participatorAnswer = null;

            if (answer == null)
            {
                participatorAnswer = new ParticipatorAnswer()
                {
                    NumberAnswer = null,
                    Question = currentQuestion
                };
            } 
            else 
            {
                IEnumerable<Answer> answers = from a in currentQuestion.Answers where a.NumericalAnswer != null && a.NumericalAnswerEpsilon != null select a;

                if (!answers.Any())
                {
                    _logger.LogWarning(ServiceLogEvents.AnswerRegistrationError,
                        "Every numerical answer or numerical answer epsilon in question {questionGuid} is null", currentQuestion.Guid);
                    return Result.Fail("Every numerical answer or numerical answer epsilon is null");
                }

                bool isCorrect = false;
                foreach (Answer a in answers) {
                    if ((Math.Abs((double)a.NumericalAnswer! - (double)answer!) < a.NumericalAnswerEpsilon) && a.IsCorrect)
                    {
                        isCorrect = true;
                        break;
                    }
                }

                participatorAnswer = new ParticipatorAnswer()
                {
                    NumberAnswer = answer,
                    Question = currentQuestion,
                    IsCorrect = isCorrect
                };
            }

            participator.Answers.Add(participatorAnswer);
            participatorRepository.UpdateParticipator(participator);
            await participatorRepository.SaveAsync();

            return Result.Ok();
        }

        public async Task<Result> RegisterTextAnswer(string userId, string lobbyGuid, string? answer)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            IParticipatorRepository participatorRepository = scope.ServiceProvider.GetRequiredService<IParticipatorRepository>();

            Result<Lobby> lobbyResult = await GetLobbyForAnswerRegistration(userId, lobbyGuid, answer, QuestionType.TextEntry);
            if (lobbyResult.IsFailed)
            {
                return Result.Fail(lobbyResult.Errors.First());
            }

            Lobby lobby = lobbyResult.Value;

            Participator? participator = GetLobbyParticipator(lobby, userId);
            if (participator == null)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register text answer {answer} because user {userId} doesn't participate in lobby {lobbyGuid}", answer, userId, lobbyGuid);
                return Result.Fail(new LobbyAccessDeniedError($"User {userId} is not part of the lobby {lobbyGuid}"));
            }

            Question? currentQuestion = lobby.GetCurrentQuestion();
            if (currentQuestion == null)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register text answer {answer} for user {userId} in {lobbyGuid} bacause currentQuestion not found (is null)", answer, userId, lobbyGuid);
                return Result.Fail(new QuizNotFoundError("The question is null. Possible reasion is that quiz in lobby is null."));
            }

            if (currentQuestion.Type != QuestionType.TextEntry)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register text answer {answer} for user {userId} in lobby {lobbyGuid} because current question type is not NumberEntry", answer, userId, lobbyGuid);
                return Result.Fail(new InvalidAnswerFormatError("Invalid answer format."));
            }

            IEnumerable<ParticipatorAnswer> currentQuestionParticipatorAnswers = from a in participator.Answers where a.Question.Guid == currentQuestion.Guid select a;
            if (currentQuestionParticipatorAnswers.Any())
            {
                return Result.Fail(new QuestionAlreadyAnsweredError("Question has already been answered."));
            }

            ParticipatorAnswer? participatorAnswer = null;

            if (answer == null)
            {
                participatorAnswer = new ParticipatorAnswer()
                {
                    TextAnswer = null,
                    Question = currentQuestion
                };
            }
            else
            {
                IEnumerable<Answer> answers = from a in currentQuestion.Answers where a.TextAnswer != null select a;

                if (!answers.Any())
                {
                    _logger.LogWarning(ServiceLogEvents.AnswerRegistrationError,
                        "Every text answer in question {questionGuid} is null", currentQuestion.Guid);
                    return Result.Fail("Every text answer is null");
                }

                bool isCorrect = false;
                foreach (Answer a in answers)
                {
                    bool isUserAnswerEqualToQuestionAnswer = answer.Trim().Equals(a.TextAnswer!.Trim(), StringComparison.CurrentCultureIgnoreCase);
                    if (isUserAnswerEqualToQuestionAnswer && a.IsCorrect)
                    {
                        isCorrect = true;
                        break;
                    }
                }

                participatorAnswer = new ParticipatorAnswer()
                {
                    TextAnswer = answer,
                    Question = currentQuestion,
                    IsCorrect = isCorrect
                };
            }

            participator.Answers.Add(participatorAnswer);
            participatorRepository.UpdateParticipator(participator);
            await participatorRepository.SaveAsync();

            return Result.Ok();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~LobbyConductService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private Participator? GetLobbyParticipator(Lobby lobby, string userId)
        {
            Participator? participator = null;
            foreach (Participator p in lobby.Participators)
            {
                if (p.UserId == userId)
                {
                    participator = p;
                }
            }
            return participator;
        }

        private async Task<Result<Lobby>> GetLobbyForAnswerRegistration(string userId, string lobbyGuid, string? answer, QuestionType type)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser? user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register {type} answer '{answer}' in lobby {lobbyGuid} because user {userId} is not found", type.ToString(), answer, lobbyGuid, userId);
                return Result.Fail(new UserNotFoundError("Invalid user ID."));
            }

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register {type} answer '{answer}' for user {userId} because lobby {lobbyGuid} is not found", type.ToString(), answer, userId, lobbyGuid);
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            if (!lobby.IsStarted)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register {type} answer '{answer}' for user {userId} because lobby {lobbyGuid} isn't started yet", type.ToString(), answer, userId, lobbyGuid);
                return Result.Fail(new LobbyUnavailableError("Lobby has't started yet."));
            }
            if (!(lobby.Stage == LobbyStage.Answering || lobby.Stage == LobbyStage.Question))
            {
                return Result.Fail(new NotRightTimeToAnswerError("Not right time to answer."));
            }

            return Result.Ok(lobby);
        }
	}
}
