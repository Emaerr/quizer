using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;
using Quizer.Models.User;
using Quizer.Services.Lobbies.impl;
using Quizer.Services.Lobbies;
using Quizer.Services.Quizzes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizerTests.Services.Lobbies.impl
{
    public class LobbyMocks
    {
        public static IServiceScopeFactory GetScopeFactoryMock(ILobbyRepository lobbyRepository)
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(ILobbyRepository)))
                .Returns(lobbyRepository);
            serviceProvider.
                Setup(x => x.GetService(typeof(IQuizRepository))).
                Returns(GetQuizRepositoryMock());
            serviceProvider
                .Setup(x => x.GetService(typeof(UserManager<ApplicationUser>)))
                .Returns(GetUserManagerMock());
            serviceProvider
                .Setup(x => x.GetService(typeof(IParticipatorRepository)))
                .Returns(new Mock<IParticipatorRepository>().Object);

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

        public static ILogger<TService> GetLoggerMock<TService>() where TService : class
        {
            var store = new Mock<ILogger<TService>>();
            return store.Object;
        }

        public static UserManager<ApplicationUser> GetUserManagerMock()
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

        public static ILobbyRepository GetLobbyWithUserRepositoryMock(bool isLobbyStarted, LobbyStage lobbyStage = LobbyStage.Question, QuestionType questionType = QuestionType.Test)
        {
            var lobby = new Lobby(lobbyStage)
            {
                IsStarted = isLobbyStarted,
                MasterId = "0",
                MaxParticipators = 10,
                Quiz = new Quiz()
                {
                    AuthorId = "0",
                    TimeLimit = 10,
                    Questions = new List<Question>() { new Question() { Guid = "0", Type = questionType, Position = 0, Answers = new List<Answer>() { new Answer() { Guid = "0", TextAnswer = "test", NumericalAnswer = 0, NumericalAnswerEpsilon = 0.1f } } }, new Question() { Guid = "1", Type = questionType, Position = 1 } }
                },
                Participators = new List<Participator>() { new Participator() { Id = "0", Answers = { new ParticipatorAnswer() { Question = new Question() { Guid = "1" } } } } },
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

        public static ILobbyRepository GetLobbyRepositoryMock(bool isLobbyStarted, QuestionType questionType = QuestionType.Test)
        {
            var lobby = new Lobby()
            {
                IsStarted = isLobbyStarted,
                MasterId = "0",
                MaxParticipators = 10,
                Quiz = new Quiz()
                {
                    AuthorId = "0",
                    TimeLimit = 10,
                    Questions = new List<Question>() { new Question() { Type = questionType, Position = 0, Answers = new List<Answer>() { new Answer() { Guid = "0", TextAnswer = "test", NumericalAnswer = 0, NumericalAnswerEpsilon = 0.1f } } }, new Question() { Type = questionType, Position = 1 } }
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

        public static IQuizRepository GetQuizRepositoryMock()
        {
            var quizRepository = new Mock<IQuizRepository>();
            quizRepository.Setup(x => x.GetQuizByGuid(It.IsAny<string>())).Returns(new Quiz()
            {
                AuthorId = "0",
                TimeLimit = 10,
            });
            return quizRepository.Object;
        }
    }
}
