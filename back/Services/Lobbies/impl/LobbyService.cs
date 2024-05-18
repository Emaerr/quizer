using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Quizer.Data;
using Quizer.Exceptions.Models;
using Quizer.Exceptions.Services;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;
using Quizer.Models.User;
using Quizer.Services.Quizzes;
using Quizer.Services.Util;
using System;
using System.Timers;

namespace Quizer.Services.Lobbies.impl
{
    public class LobbyService : ILobbyControlService, ILobbyConductService, ILobbyAuthService, IHostedService, IDisposable
    {
        private bool disposedValue;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITimeService _timeService;
        private readonly ILogger<LobbyService> _logger;

        public LobbyService(IServiceScopeFactory scopeFactory, ITimeService timeService, ILogger<LobbyService> logger)
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
            } else
            {
                return LobbyStatus.Briefing;
            }
        }

        public async Task<Result<QuestionData>> GetCurrentQuestion(string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

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

            ParticipatorAnswer participatorAnswer = new ParticipatorAnswer() {
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

        public Task<Result<IEnumerable<AnswerData>>> GetRightAnswers(string lobbyGuid)
        {
            throw new NotImplementedException();
        }
        public async Task<Result<bool>> IsUserMaster(string userId, string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser? user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail(new UserNotFoundError("Invalid joining user id."));
            }

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            if (lobby.MasterId == user.Id) {
                return true;
            } else
            {
                return false;
            }
        }

        public async Task<Result<bool>> IsUserParticipator(string userId, string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser? user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail(new UserNotFoundError("Invalid joining user id."));
            }

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            foreach (Participator participator in lobby.Participators)
            {
                if (participator.Id == userId)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<Result<string>> CreateAsync(string masterId, string quizGuid, int maxParticipators)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();

            int pin = GenerateRandomPin(5);
            while (!CheckIfPinIsUnique(pin))
            {
                pin = GenerateRandomPin(5);
            }

            Quiz? quiz = quizRepository.GetQuizByGuid(quizGuid);

            if (quiz == null)
            {
                return Result.Fail(new QuizNotFoundError("Quiz not found"));
            }

            Lobby lobby = new Lobby()
            {
                MasterId = masterId,
                Quiz = quiz,
                MaxParticipators = maxParticipators,
                Pin = pin,
            };
            lobbyRepository.InsertLobby(lobby);
            await lobbyRepository.SaveAsync();

            if (lobby.Guid != null) {
                return Result.Ok(lobby.Guid);
            } else
            {
                return Result.Fail("Impossible happened. Lobby GUID is null. (check lobby constructor)");
            }
        }

        public Result ForceNextQuestionAsync(string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            lobby.NextQuestion();
            lobby.ResetTime();
            return Result.Ok();            
        }

        public async Task<Result> JoinUserAsync(string lobbyGuid, string joiningUserId)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            IParticipatorRepository participatorRepository = scope.ServiceProvider.GetRequiredService<IParticipatorRepository>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser? user = await userManager.FindByIdAsync(joiningUserId);
            if (user == null)
            {
                return Result.Fail(new UserNotFoundError("Invalid joining user id."));
            }

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            Participator participator = new Participator(joiningUserId);
            participatorRepository.InsertParticipator(participator);
            participatorRepository.Save();

            if (lobby.IsStarted)
            {
                return Result.Fail(new LobbyUnavailableError("The game has already started."));
            }

            if (lobby.Participators.Count() <= lobby.MaxParticipators)
            {
                lobby.Participators.Add(participator);
            }
            else
            {
                return Result.Fail(new MaxParticipatorsError("No free player slot in the lobby."));
            }

            lobbyRepository.UpdateLobby(lobby);
            await lobbyRepository.SaveAsync();

            return Result.Ok();
        }

        public async Task<Result> KickUserAsync(string lobbyGuid, string userToKickId)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("(Invalid lobby GUID."));
            }

            foreach (Participator participator in lobby.Participators)
            {
                if (participator.Id == userToKickId)
                {
                    lobby.Participators.Remove(participator);
                }
            }

            lobbyRepository.UpdateLobby(lobby);
            await lobbyRepository.SaveAsync();

            return Result.Ok();
        }

        public async Task<Result> StartLobbyAsync(string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            lobby.IsStarted = true;

            lobbyRepository.UpdateLobby(lobby);
            await lobbyRepository.SaveAsync();

            return Result.Ok();
        }

        public async Task<Result> StopLobbyAsync(string lobbyGuid)
        {

            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            IParticipatorRepository participatorRepository = scope.ServiceProvider.GetRequiredService<IParticipatorRepository>();

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            lobby.IsStarted = false;

            foreach (Participator participator in lobby.Participators)
            {
                participatorRepository.DeleteParticipator(participator.Id);
            }

            await participatorRepository.SaveAsync();

            lobbyRepository.DeleteLobby(lobby.Id);
            await lobbyRepository.SaveAsync();

            return Result.Ok();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            DateTime time = _timeService.GetDateTimeNow();

            while (!cancellationToken.IsCancellationRequested)
            {
                DateTime now = _timeService.GetDateTimeNow();
                TimeSpan timeSpan = now - time;
                time = now;

                IServiceScope scope = _scopeFactory.CreateScope();
                ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();

                IEnumerable<Lobby> lobbies = lobbyRepository.GetLobbies();

                foreach (Lobby lobby in lobbies)
                {
                    lobby.Update(timeSpan);
                }
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            IEnumerable<Lobby> lobbies = lobbyRepository.GetLobbies();

            foreach (Lobby lobby in lobbies)
            {
                lobby.IsStarted = false;
            }

            return Task.CompletedTask;
        }

        public async Task<Result<List<ApplicationUser>>> GetUsersInLobby(string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError($"Invalid lobby GUID {lobbyGuid}"));
            }

            List<ApplicationUser> users = new List<ApplicationUser>();
            foreach(Participator participator in lobby.Participators)
            {
                ApplicationUser? userParticipating = await userManager.FindByIdAsync(participator.Id);

                if (userParticipating == null)
                {
                    return Result.Fail(new UserNotFoundError($"Invalid participating user id {participator.Id}"));
                }

                users.Add(userParticipating);
            }

            return users;

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
        // ~QuizService()
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

        /// <summary>
        /// Generates random pin of fixed length.
        /// </summary>
        /// <param name="length">Pin length</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private int GenerateRandomPin(int length)
        {
            if (length > 9)
            {
                throw new Exception("GenerateUniquePin: Length should not be higher than 9");
            }

            int[] ints = new int[length];

            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                ints[i] = random.Next(1, 9);
            }

            int pin = 0;

            double digitPlace = 1;

            for (int i = 0; i < length; i++)
            {
                pin += ints[i] * (int)digitPlace;
                digitPlace *= 10;
            }

            return pin;
        }

        private bool CheckIfPinIsUnique(int pin)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();

            IEnumerable<Lobby> lobbies = lobbyRepository.GetLobbies();

            foreach (Lobby lobby in lobbies) {
                if (lobby.Pin == pin) { 
                    return false;
                }
            }

            return true;
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
