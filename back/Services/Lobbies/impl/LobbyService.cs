﻿using FluentResults;
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
using static Quizer.Models.Lobbies.Lobby;
using static Quizer.Services.Lobbies.ILobbyConductService;
using static Quizer.Services.Lobbies.ILobbyUpdateService;

namespace Quizer.Services.Lobbies.impl
{
    public class LobbyService : BackgroundService, IDisposable, ILobbyUpdateService
    {
        private bool disposedValue;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITimeService _timeService;
        private readonly ILogger<LobbyService> _logger;
        private readonly int _answerTime = 1000;

        private Dictionary<string, int> lobbiesTimeElapsedSinceLastAction = new Dictionary<string, int>();
        private Dictionary<string, LobbyStatusUpdateHandler> lobbyUpdateHandlers = new Dictionary<string, LobbyStatusUpdateHandler>();

        public LobbyService(IServiceScopeFactory scopeFactory, ITimeService timeService, IConfiguration configuration, ILogger<LobbyService> logger)
        {
            _scopeFactory = scopeFactory;
            _timeService = timeService;
            _logger = logger;
            try
            {
                string? answerTimeStr = configuration["AnswerTimeInMilliseconds"];
                if (answerTimeStr == null)
                {
                    throw new ArgumentNullException(nameof(answerTimeStr));
                }
                int result = Int32.Parse(answerTimeStr);
            }
            catch (FormatException)
            {
                logger.LogError("Can't parse AnswerTimeInMilliseconds from config.");
            }
            catch (ArgumentNullException)
            {
                logger.LogError("Can't find AnswerTimeInMilliseconds in config.");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using PeriodicTimer timer = new(TimeSpan.FromSeconds(0.1));

            try
            {
                DateTime time = _timeService.GetDateTimeNow();

                while (await timer.WaitForNextTickAsync(cancellationToken))
                {
                    DateTime now = _timeService.GetDateTimeNow();
                    TimeSpan timeSpan = now - time;
                    time = now;

                    IServiceScope scope = _scopeFactory.CreateScope();
                    ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();

                    IEnumerable<Lobby> lobbies = await lobbyRepository.GetLobbiesAsync();

                    foreach (Lobby lobby in lobbies)
                    {
                        if (lobby.IsStarted)
                        {
                            UpdateLobby(lobby, timeSpan);
                            lobbyRepository.UpdateLobby(lobby);
                        }
                    }

                    await lobbyRepository.SaveAsync();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Timed Hosted Service is stopping.");
            }
        }

        public void UpdateLobby(Lobby lobby, TimeSpan timeSpan)
        {
            checked
            {
                try
                {
                    if (!lobbiesTimeElapsedSinceLastAction.ContainsKey(lobby.Guid))
                    {
                        lobbiesTimeElapsedSinceLastAction[lobby.Guid] = 0;
                    }
                    lobbiesTimeElapsedSinceLastAction[lobby.Guid] += (int)timeSpan.TotalMilliseconds;
                }
                catch (OverflowException e)
                {
                    throw new ModelException("Given time span is too high.", e);
                }
            }

            if (!lobbyUpdateHandlers.TryGetValue(lobby.Guid, out LobbyStatusUpdateHandler onLobbyStageChange)) {
                return;
            }

            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();

            if (lobby.IsBriefingTime())
            {
                lobby.Stage = LobbyStage.Question;
                if (onLobbyStageChange != null)
                {
                    _logger.LogInformation($"Lobby {lobby.Guid} changed it's stage to Question");
                    onLobbyStageChange(LobbyStatus.Question);
                }
            }

            if (lobby.IsQuestionTime())
            {
                if (lobbiesTimeElapsedSinceLastAction[lobby.Guid] > lobby.Quiz.TimeLimit)
                {
                    _logger.LogInformation($"Lobby {lobby.Guid} changed it's stage to Answering");
                    lobby.Stage = LobbyStage.Answering;
                    lobbiesTimeElapsedSinceLastAction[lobby.Guid] = lobbiesTimeElapsedSinceLastAction[lobby.Guid] - lobby.Quiz.TimeLimit;
                    if (onLobbyStageChange != null)
                    {
                        onLobbyStageChange(LobbyStatus.Answering);
                    }
                }
            }
            else if (lobby.IsAnsweringTime())
            {
                if (lobbiesTimeElapsedSinceLastAction[lobby.Guid] > _answerTime)
                {
                    if (lobby.CurrentQuestionPosition == (lobby.Quiz.Questions.Count - 1))
                    {
                        _logger.LogInformation($"Lobby {lobby.Guid} changed it's stage to Results");
                        lobby.Stage = LobbyStage.Results;
                        lobby.IsStarted = false;
                        if (onLobbyStageChange != null)
                        {
                            onLobbyStageChange(LobbyStatus.Result);
                        }
                    } else
                    {
                        _logger.LogInformation($"Lobby {lobby.Guid} changed it's stage to Break");
                        lobby.Stage = LobbyStage.Break;
                        lobbiesTimeElapsedSinceLastAction[lobby.Guid] = lobbiesTimeElapsedSinceLastAction[lobby.Guid] - 1000;
                        if (onLobbyStageChange != null)
                        {
                            //onLobbyStageChange(LobbyStatus.Break);
                        }
                    }
                }
            }
            else if (lobby.IsBreakTime())
            {
                if (lobbiesTimeElapsedSinceLastAction[lobby.Guid] > 0) //TODO lobby.Quiz.BreakTime
                {
                    lobby.NextQuestion();
                    if (onLobbyStageChange != null)
                    {
                        onLobbyStageChange(LobbyStatus.Question);
                    }
                    //lobbiesTimeElapsedSinceLastAction[lobby.Guid] = lobbiesTimeElapsedSinceLastAction[lobby.Guid] - lobby.Quiz.BreakTime;
                    lobbiesTimeElapsedSinceLastAction[lobby.Guid] = 0;
                }
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
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
        public Result SubscribeToLobbyStatusUpdateEvent(string lobbyGuid, LobbyStatusUpdateHandler handler)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyRepository lobbyRepository = scope.ServiceProvider.GetRequiredService<ILobbyRepository>();

            Lobby? lobby = lobbyRepository.GetLobbyByGuid(lobbyGuid);
            if (lobby == null)
            {
                return Result.Fail(new LobbyNotFoundError("Invalid lobby GUID."));
            }

            lobbyUpdateHandlers[lobby.Guid] = handler;

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
    }
}
