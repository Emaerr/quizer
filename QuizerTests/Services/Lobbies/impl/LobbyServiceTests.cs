using Castle.Components.DictionaryAdapter.Xml;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;
using Quizer.Models.User;
using Quizer.Services.Lobbies.impl;
using Quizer.Services.Quizzes;
using Quizer.Services.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quizer.Services.Lobbies.impl.Tests
{
    [TestClass()]
    public class LobbyServiceTests
    {

        [TestMethod()]
        public async Task CreateAsyncTestAsync()
        {
            ILobbyControlService service = new LobbyService(GetScopeFactoryMock(), new TestTimeService(), GetLoggerMock());
            Result<string> result = await service.CreateAsync("0", "0", 10);
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public async Task ForceNextQuestionAsyncTestAsync()
        {
            ILobbyControlService service = new LobbyService(GetScopeFactoryMock(), new TestTimeService(), GetLoggerMock());
            Result<string> result = await service.ForceNextQuestionAsync("0", "0");
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public async Task JoinUserAsyncTestAsync()
        {
            ILobbyControlService service = new LobbyService(GetScopeFactoryMock(), new TestTimeService(), GetLoggerMock());
            Result<string> result = await service.JoinUserAsync("0", "1");
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public async Task KickUserAsyncTestAsync()
        {
            ILobbyControlService service = new LobbyService(GetScopeFactoryMock(), new TestTimeService(), GetLoggerMock());
            Result<string> result = await service.KickUserAsync("0", "0", "1");
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public async Task StartLobbyAsyncTestAsync()
        {
            ILobbyControlService service = new LobbyService(GetScopeFactoryMock(), new TestTimeService(), GetLoggerMock());
            Result<string> result = await service.StartLobbyAsync("0", "0");
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public async Task StopLobbyAsyncTestAsync()
        {
            ILobbyControlService service = new LobbyService(GetScopeFactoryMock(), new TestTimeService(), GetLoggerMock());
            Result<string> result = await service.StopLobbyAsync("0", "0");
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        //public async Task StartAsyncTestAsync()
        //{
        //    var scopeFactory = GetScopeFactoryMock();
        //    TestTimeService timeService = new TestTimeService();
        //    IHostedService service = new LobbyService(scopeFactory, timeService, GetLoggerMock());
        //    timeService.SetDateTime(new DateTime(1, 1, 1, 1, 1, 0));
        //    Task.Run(() => service.StartAsync(new CancellationToken()));
        //    timeService.SetDateTime(new DateTime(1, 1, 1, 1, 1, 2));

        //    Thread.Sleep(100); //TODO: Get rid of thi
        //    ILobbyRepository repository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILobbyRepository>();
        //    Lobby? lobby = repository.GetLobbyByGuid("0");
        //    Assert.IsNotNull(lobby);
        //    Question? currentQuestion = lobby.GetCurrentQuestion();
        //    Assert.IsNotNull(currentQuestion);
        //    Assert.AreEqual(currentQuestion.Position, 0);

        //    timeService.SetDateTime(new DateTime(1, 1, 1, 1, 1, 10));
        //    Thread.Sleep(100); //TODO: Get rid of this
        //    lobby = repository.GetLobbyByGuid("0");
        //    Assert.IsNotNull(lobby);
        //    currentQuestion = lobby.GetCurrentQuestion();
        //    Assert.IsNotNull(currentQuestion);
        //    Assert.AreEqual(currentQuestion.Position, 1);

        //    Task.Run(() => service.StopAsync(new CancellationToken()));
        //}

        private IServiceScopeFactory GetScopeFactoryMock()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(ILobbyRepository)))
                .Returns(GetLobbyRepositoryMock());
            serviceProvider.
                Setup(x => x.GetService(typeof(IQuizRepository))).
                Returns(GetQuizRepositoryMock());
            serviceProvider
                .Setup(x => x.GetService(typeof(UserManager<ApplicationUser>)))
                .Returns(GetUserManagerMock());

            var serviceScope = new Mock<IServiceScope>();
            serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            serviceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(serviceScope.Object);

            serviceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactory.Object);

            return serviceScopeFactory.Object;
        }

        private ILogger<LobbyService> GetLoggerMock()
        {
            var store = new Mock<ILogger<LobbyService>>();
            return store.Object;
        }

        private UserManager<ApplicationUser> GetUserManagerMock()
        {
            List<ApplicationUser> users = new List<ApplicationUser>()
            {
                new ApplicationUser()
                {
                    Id = "0",
                    UserName = "test",
                    DisplayName = "Test"
                },
                new ApplicationUser()
                {
                    Id = "1",
                    UserName = "test1",
                    DisplayName = "Test1"
                }
            };
            UserManager<ApplicationUser> userManager = MockUserManager<ApplicationUser>(users).Object;

            return userManager;
        }

        public static Mock<UserManager<TUser>> MockUserManager<TUser>(List<TUser> ls) where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<TUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

            mgr.Setup(x => x.DeleteAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<TUser, string>((x, y) => ls.Add(x));
            mgr.Setup(x => x.UpdateAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(ls.First());

            return mgr;
        }

        private ILobbyRepository GetLobbyRepositoryMock()
        {
            var lobby = new Lobby()
            {
                IsStarted = false,
                MasterId = "0",
                MaxParticipators = 10,
                Quiz = new Models.Quizzes.Quiz()
                {
                    AuthorId = "0",
                    TimeLimit = 10,
                    Questions = new List<Question>() { new Question() { Position = 0 }, new Question() { Position = 1 } }
                }
            };

            var lobbyRepository = new Mock<ILobbyRepository>();
            lobbyRepository.Setup(x => x.InsertLobby(It.IsAny<Lobby>()));
            lobbyRepository.Setup(x => x.UpdateLobby(It.IsAny<Lobby>()));
            lobbyRepository.Setup(x => x.DeleteLobby(It.IsAny<int>()));
            lobbyRepository.Setup(x => x.Save());
            lobbyRepository.Setup(x => x.SaveAsync());
            lobbyRepository.Setup(x => x.GetLobbyByGuid(It.IsAny<string>())).Returns(lobby);

            return lobbyRepository.Object;
        }

        private IQuizRepository GetQuizRepositoryMock()
        {
            var quizRepository = new Mock<IQuizRepository>();
            quizRepository.Setup(x => x.GetQuizByGuid(It.IsAny<string>())).Returns(new Models.Quizzes.Quiz()
            {
                AuthorId = "0",
                TimeLimit = 10,
            });
            return quizRepository.Object;
        }
    }
}