using FluentResults;
using Microsoft.AspNetCore.Identity;
using Quizer.Exceptions.Services;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;
using Quizer.Models.User;
using Quizer.Services.Quizzes;
using Quizer.Services.Util;

namespace Quizer.Services.Lobbies.impl
{
    public class LobbyConductService : ILobbyConductService, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITimeService _timeService;
        private readonly ILogger<LobbyConductService> _logger;
        private bool disposedValue;

        public LobbyConductService(IServiceScopeFactory scopeFactory, ITimeService timeService, ILogger<LobbyConductService> logger)
        {
            _scopeFactory = scopeFactory;
            _timeService = timeService;
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

            if (lobby.IsResultTime())
            {
                return LobbyStatus.Results;
            }
            if (lobby.IsStarted)
            {
                return LobbyStatus.Game;
            }
            else
            {
                return LobbyStatus.Briefing;
            }
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
                return Result.Fail(new LobbyUnavailableError("Lobby has't started yet."));
            }
            if (lobby.IsResultTime())
            {
                return Result.Fail(new LobbyUnavailableError("Game has already finished."));
            }

            Question? question = lobby.GetCurrentQuestion();
            if (question == null)
            {
                return Result.Fail(new QuizNotFoundError("The question is null. Possible reasion is that quiz in lobby is null."));
            }

            return Result.Ok(question);
        }

        public async Task<Result> RegisterTestAnswer(string userId, string lobbyGuid, string? answerGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            IParticipatorRepository participatorRepository = scope.ServiceProvider.GetRequiredService<IParticipatorRepository>();

            ApplicationUser? user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register test answer {answerGuid} in lobby {lobbyGuid} because user {userId} is not found",answerGuid, lobbyGuid, userId);
                return Result.Fail(new UserNotFoundError("Invalid user ID."));
            }

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register test answer {answerGuid} for user {userId} because lobby {lobbyGuid} is not found", answerGuid, userId, lobbyGuid);
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            if (!lobby.IsStarted)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register test answer {answerGuid} for user {userId} because lobby {lobbyGuid} isn't started yet", answerGuid, userId, lobbyGuid);
                return Result.Fail(new LobbyUnavailableError("Lobby has't started yet."));
            }
            if (lobby.IsResultTime())
            {
                return Result.Fail(new LobbyUnavailableError("Game has already finished."));
            }

            Participator? participator = null;
            foreach (Participator p in lobby.Participators)
            {
                if (p.Id == userId)
                {
                    participator = p;
                }
            }
            if (participator == null)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register test answer {answerGuid} because user {userId} doesn't participate in lobby {lobbyGuid}", answerGuid, userId, lobbyGuid);
                return Result.Fail(new LobbyAccessDeniedError($"User {userId} is not part of the lobby {lobbyGuid}"));
            }

            Question? currentQuestion = lobby.GetCurrentQuestion();
            if (currentQuestion == null)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register test answer {answerGuid} for user {userId} {lobbyGuid} bacause currentQuestion not found (is null)", answerGuid, userId, lobbyGuid);
                return Result.Fail(new QuizNotFoundError("The question is null. Possible reasion is that quiz in lobby is null."));
            }

            if (currentQuestion.Type != QuestionType.Test)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register test answer {answerGuid} for user {userId} in lobby {lobbyGuid} because current question type is not test", answerGuid, userId, lobbyGuid);
                return Result.Fail(new InvalidAnswerFormatError("Invalid answer format."));
            }

            bool isAnswerGuidValid = false;
            bool isAnswerCorrect = false;

            foreach (Answer answer in currentQuestion.Answers)
            {
                if (answer.Guid == answerGuid)
                {
                    isAnswerGuidValid = true;

                    if (answer.IsCorrect)
                    {
                        isAnswerCorrect = true;
                    }
                }
            }

            if (!isAnswerGuidValid)
            {
                _logger.LogInformation(ServiceLogEvents.AnswerRegistrationError, "Couldn't register test answer {answerGuid} for user {userId} in lobby {lobbyGuid} because the answer not found (answer GUID is not valid)", answerGuid, userId, lobbyGuid);
                return Result.Fail(new InvalidAnswerGuidError("Invalid answer GUID"));
            }

            ParticipatorAnswer participatorAnswer = new ParticipatorAnswer()
            {
                TestAnswerGuid = answerGuid,
            };

            participator.Answers.Add(participatorAnswer);
            
            if (isAnswerCorrect)
            {
                participator.Points++;
            }

            await participatorRepository.SaveAsync();

            _logger.LogInformation(ServiceLogEvents.AnswerRegistered, "Succesfully registered test answer {answerGuid} for user {userId} in lobby {lobbyGuid}", answerGuid, userId, lobbyGuid);

            return Result.Ok();
        }

        public Task<Result> RegisterNumericalAnswer(string userId, string lobbyGuid, float answer)
        {
            throw new NotImplementedException();
        }

        public Task<Result> RegisterTextAnswer(string userId, string lobbyGuid, string answer)
        {
            throw new NotImplementedException();
        }

        public Result<IEnumerable<Answer>> GetRightAnswers(string lobbyGuid)
        {
            throw new NotImplementedException();
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

        private QuestionData GetQuestionDataFromQuestion(Question question)
        {
            List<AnswerData> answers = [];
            foreach (Answer answer in question.Answers)
            {
                AnswerInfo answerInfo = new AnswerInfo(answer.Title, answer.IsCorrect);
                answers.Add(new AnswerData(answer.Guid, answerInfo));
            }

            QuestionInfo info = new QuestionInfo(question.Position, question.Title, question.Type);

            return new QuestionData(question.Guid, info, answers);
        }
    }
}
