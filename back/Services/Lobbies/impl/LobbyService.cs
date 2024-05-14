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
    public class LobbyService : ILobbyControlService, ILobbyConductService, IHostedService, IDisposable
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

        public async Task<Result<QuestionData>> GetCurrentQuestion(string userId, string lobbyGuid)
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

            Question? question = lobby.GetCurrentQuestion();
            if (question == null)
            {
                return Result.Fail(new QuizNotFoundError("The question is null. Possible reasiob is that quiz in lobby is null."));
            }

            return Result.Ok(GetQuestionDataFromQuestion(question));
        }

        public async Task<Result<string>> CreateAsync(string masterId, string quizGuid, int maxParticipators)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();

            Lobby lobby = new Lobby()
            {
                MasterId = masterId,
                Quiz = quizRepository.GetQuizByGuid(quizGuid),
                MaxParticipators = maxParticipators,
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

        public async Task<Result> ForceNextQuestionAsync(string userId, string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
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

            lobby.NextQuestion();
            lobby.ResetTime();
            return Result.Ok();            
        }

        public async Task<Result> JoinUserAsync(string lobbyGuid, string joiningUserId)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
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

            return Result.Ok();
        }

        public async Task<Result> KickUserAsync(string userId, string lobbyGuid, string userToKickId)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser? user = await userManager.FindByIdAsync(userToKickId);
            if (user == null)
            {
                return Result.Fail(new UserNotFoundError("Invalid user to kick id."));
            }

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

            return Result.Ok();
        }

        public async Task<Result> StartLobbyAsync(string userId, string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser? user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail(new UserNotFoundError("Invalid user id."));
            }

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            lobby.IsStarted = true;

            return Result.Ok();
        }

        public async Task<Result> StopLobbyAsync(string userId, string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser? user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail(new UserNotFoundError("Invalid user id."));
            }

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            lobby.IsStarted = false;
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

        public async Task<Result<List<ApplicationUser>>> GetLobbyParticipants(string userId, string lobbyGuid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser? user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail(new UserNotFoundError($"Invalid user id {userId}"));
            }

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

        private QuestionData GetQuestionDataFromQuestion(Question question)
        {
            List<AnswerData> answers = [];
            foreach (Answer answer in question.Answers)
            {
                AnswerInfo answerInfo = new AnswerInfo(answer.Title, answer.IsCorrect);
                answers.Add(new AnswerData(answer.Guid, answerInfo));
            }

            QuestionInfo info = new QuestionInfo(question.Position, question.Title);

            return new QuestionData(question.Guid, info, answers);
        }
    }
}
