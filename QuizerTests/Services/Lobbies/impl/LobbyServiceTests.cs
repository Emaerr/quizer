using Castle.Components.DictionaryAdapter.Xml;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Quizer.Models.Lobbies;
using Quizer.Models.User;
using Quizer.Services.Lobbies.impl;
using Quizer.Services.Quizzes;
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
        public async Task CreateTestAsync()
        {
            ILobbyControlService service = new LobbyService(GetScopeFactoryMock(), GetQuizControlServiceMock(), GetUserManagerMock());
            Result<string> result = await service.CreateAsync("0", "0", 10);
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public async Task ForceNextQuestionTestAsync()
        {
            ILobbyControlService service = new LobbyService(GetScopeFactoryMock(), GetQuizControlServiceMock(), GetUserManagerMock());
            Result<string> result = await service.ForceNextQuestionAsync("0", "0");
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public async Task JoinUserTestAsync()
        {
            ILobbyControlService service = new LobbyService(GetScopeFactoryMock(), GetQuizControlServiceMock(), GetUserManagerMock());
            Result<string> result = await service.JoinUserAsync("0", "1");
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public async Task KickUserTestAsync()
        {
            ILobbyControlService service = new LobbyService(GetScopeFactoryMock(), GetQuizControlServiceMock(), GetUserManagerMock());
            Result<string> result = await service.KickUserAsync("0", "0", "1");
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public async Task StartTestAsync()
        {
            ILobbyControlService service = new LobbyService(GetScopeFactoryMock(), GetQuizControlServiceMock(), GetUserManagerMock());
            Result<string> result = await service.StartLobbyAsync("0", "0");
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public async Task StopTestAsync()
        {
            ILobbyControlService service = new LobbyService(GetScopeFactoryMock(), GetQuizControlServiceMock(), GetUserManagerMock());
            Result<string> result = await service.StopLobbyAsync("0", "0");
            Assert.IsTrue(result.IsSuccess);
        }

        private IServiceScopeFactory GetScopeFactoryMock()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(ILobbyRepository)))
                .Returns(GetLobbyRepositoryMock());
            serviceProvider.
                Setup(x => x.GetService(typeof(IQuizRepository))).
                Returns(GetQuizRepositoryMock());

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

        private ILobbyControlService GetQuizControlServiceMock()
        {
            var quizControlService = new Mock<ILobbyControlService>();
            quizControlService.Setup(x => x.ForceNextQuestion(It.IsAny<string>()));
            return quizControlService.Object;
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
            var lobbyRepository = new Mock<ILobbyRepository>();
            lobbyRepository.Setup(x => x.InsertLobby(It.IsAny<Lobby>()));
            lobbyRepository.Setup(x => x.UpdateLobby(It.IsAny<Lobby>()));
            lobbyRepository.Setup(x => x.DeleteLobby(It.IsAny<int>()));
            lobbyRepository.Setup(x => x.Save());
            lobbyRepository.Setup(x => x.SaveAsync());
            lobbyRepository.Setup(x => x.GetLobbyByGuid(It.IsAny<string>())).Returns(new Lobby()
            {
                IsStarted = false,
                MasterId = "0",
                MaxParticipators = 10,
                Quiz = new Models.Quizzes.Quiz()
                {
                    AuthorId = "0",
                    TimeLimit = 10,
                }
            });

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