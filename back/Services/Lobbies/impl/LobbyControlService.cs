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
    public class LobbyControlService : ILobbyControlService, IDisposable
    {
        private bool disposedValue;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITimeService _timeService;
        private readonly ILogger<LobbyService> _logger;

        public LobbyControlService(IServiceScopeFactory scopeFactory, ITimeService timeService, ILogger<LobbyService> logger)
        {
            _scopeFactory = scopeFactory;
            _timeService = timeService;
            _logger = logger;
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

            if (lobby.Guid != null)
            {
                return Result.Ok(lobby.Guid);
            }
            else
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
            foreach (Participator participator in lobby.Participators)
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
        // ~LobbyControlService()
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

            foreach (Lobby lobby in lobbies)
            {
                if (lobby.Pin == pin)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
