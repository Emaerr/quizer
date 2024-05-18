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
        private readonly ILogger<LobbyService> _logger;
        private bool disposedValue;

        public LobbyConductService(IServiceScopeFactory scopeFactory, ITimeService timeService, ILogger<LobbyService> logger)
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

            if (lobby.IsStarted)
            {
                return LobbyStatus.Game;
            }
            else
            {
                return LobbyStatus.Briefing;
            }
        }

        public Result<QuestionData> GetCurrentQuestion(string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            Question? question = lobby.GetCurrentQuestion();
            if (question == null)
            {
                return Result.Fail(new QuizNotFoundError("The question is null. Possible reasion is that quiz in lobby is null."));
            }

            return Result.Ok(GetQuestionDataFromQuestion(question));
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
                return Result.Fail(new UserNotFoundError("Invalid user ID."));
            }

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
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
                return Result.Fail(new LobbyUnavailableError($"User {userId} is not part of the lobby {lobbyGuid}"));
            }

            Question? currentQuestion = lobby.GetCurrentQuestion();
            if (currentQuestion == null)
            {
                return Result.Fail(new QuizNotFoundError("The question is null. Possible reasion is that quiz in lobby is null."));
            }

            if (currentQuestion.Type != QuestionType.Test)
            {
                return Result.Fail(new InvalidAnswerFormatError("Invalid answer format."));
            }

            bool isAnswerGuidValid = false;

            foreach (Answer answer in currentQuestion.Answers)
            {
                if (answer.Guid == answerGuid)
                {
                    isAnswerGuidValid = true;
                }
            }

            if (!isAnswerGuidValid)
            {
                return Result.Fail(new InvalidAnswerGuidError("Invalid answer GUID"));
            }

            ParticipatorAnswer participatorAnswer = new ParticipatorAnswer()
            {
                TestAnswerGuid = answerGuid,
            };

            participator.Answers.Add(participatorAnswer);
            await participatorRepository.SaveAsync();

            return Result.Ok();
        }

        public Task<Result> RegisterNumericalAnswer(string userId, string lobbyGuid, int answer)
        {
            throw new NotImplementedException();
        }

        public Task<Result> RegisterTextAnswer(string userId, string lobbyGuid, string answer)
        {
            throw new NotImplementedException();
        }

        public Result<IEnumerable<AnswerData>> GetRightAnswers(string lobbyGuid)
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
