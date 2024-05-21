using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Quizer.Controllers;
using Quizer.Exceptions.Services;
using Quizer.Models.Quizzes;
using Quizer.Models.User;
using Quizer.Services.Lobbies;
using Quizer.Services.Lobbies.impl;
using Quizer.Services.Qr;
using Quizer.Services.Quizzes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Quizer.Controllers.Tests
{
    [TestClass()]
    public class LobbyControllerTests
    {
        [TestMethod()]
        public async Task JoinTest()
        {
            LobbyController lobbyController = new LobbyController(
                GetLoggerMock(),
                GetLobbyControlServiceMock(isLobbyStarted: false),
                GetLobbyConductServiceMock(isLobbyStarted: false),
                GetLobbyAuthServiceMock(),
                GetQrServiceMock(),
                GetUserManagerMock("0"));

            var viewResult = await lobbyController.Join("0");
            Assert.IsInstanceOfType(viewResult, typeof(RedirectToActionResult));

            lobbyController = new LobbyController(
                GetLoggerMock(),
                GetLobbyControlServiceMock(isLobbyStarted: true),
                GetLobbyConductServiceMock(isLobbyStarted: true),
                GetLobbyAuthServiceMock(),
                GetQrServiceMock(),
                GetUserManagerMock("0"));

            viewResult = await lobbyController.Join("0");
            Assert.IsInstanceOfType(viewResult, typeof(ViewResult));
        }

        [TestMethod()]
        public async Task GameTest()
        {
            LobbyController lobbyController = new LobbyController(
                GetLoggerMock(),
                GetLobbyControlServiceMock(isLobbyStarted: true),
                GetLobbyConductServiceMock(isLobbyStarted: true),
                GetLobbyAuthServiceMock(),
                GetQrServiceMock(),
                GetUserManagerMock("1"));

            var viewResult = await lobbyController.Game("0");

            Assert.IsInstanceOfType(viewResult, typeof(ViewResult));
        }

        private ILobbyConductService GetLobbyConductServiceMock(bool isLobbyStarted)
        {
            var mock = new Mock<ILobbyConductService>();

            if (isLobbyStarted)
            {
                mock.Setup(x => x.GetLobbyStatus(It.IsAny<string>())).Returns(Result.Ok(LobbyStatus.Game));
            }
            else
            {
                mock.Setup(x => x.GetLobbyStatus(It.IsAny<string>())).Returns(Result.Ok(LobbyStatus.Briefing));
            }

            mock.Setup(x => x.GetCurrentQuestion(It.IsAny<string>())).Returns(Result.Ok(new QuestionData("0", new QuestionInfo(0, "test", QuestionType.Test), [])));
            mock.Setup(x => x.RegisterTestAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Ok()));
            mock.Setup(x => x.RegisterTextAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Ok()));
            mock.Setup(x => x.RegisterNumericalAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<float>())).Returns(Task.FromResult(Result.Ok()));
            mock.Setup(x => x.GetRightAnswers(It.IsAny<string>())).
                Returns(Result.Ok((IEnumerable<AnswerData>)new List<AnswerData>() { new AnswerData("0", new AnswerInfo("test_answer", true)) }));

            return mock.Object;
        }

        private ILobbyAuthService GetLobbyAuthServiceMock()
        {
            var mock = new Mock<ILobbyAuthService>();

            mock.Setup(x => x.IsUserMaster(It.IsAny<string>(), It.IsAny<string>())).Returns((string userId, string lobbyGuid) =>
            {
                if (userId == "0" && lobbyGuid == "0")
                {
                    return Task.FromResult(Result.Ok(true));
                }
                else if (userId == "1" && lobbyGuid == "0")
                {
                    return Task.FromResult(Result.Ok(false));
                }
                return Task.FromResult(Result.Fail<bool>(""));
            });

            mock.Setup(x => x.IsUserParticipator(It.IsAny<string>(), It.IsAny<string>())).Returns((string userId, string lobbyGuid) =>
            {
                if (userId == "1" && lobbyGuid == "0")
                {
                    return Task.FromResult(Result.Ok(true));
                }
                else if (userId == "2" && lobbyGuid == "0")
                {
                    return Task.FromResult(Result.Ok(false));
                }
                return Task.FromResult(Result.Fail<bool>(""));
            });

            return mock.Object;
        }

        private ILobbyControlService GetLobbyControlServiceMock(bool isLobbyStarted)
        {
            var mock = new Mock<ILobbyControlService>();
            mock.Setup(x => x.StartLobbyAsync(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok()));
            mock.Setup(x => x.StopLobbyAsync(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok()));
            mock.Setup(x => x.GetUsersInLobby(It.IsAny<string>())).Returns(Task.FromResult(Result.Ok(new List<ApplicationUser>() { new ApplicationUser() { Id = "0" } })));
            mock.Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(Result.Ok("0")));
            if (isLobbyStarted)
            {
                mock.Setup(x => x.JoinUserAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Fail(new LobbyUnavailableError("Lobby if full."))));
            }
            else
            {
                mock.Setup(x => x.JoinUserAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Ok()));
            }
            mock.Setup(x => x.KickUserAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Ok()));
            mock.Setup(x => x.ForceNextQuestionAsync(It.IsAny<string>())).Returns(Result.Ok());

            return mock.Object;
        }

        private IQrService GetQrServiceMock()
        {
            var mock = new Mock<IQrService>();

            mock.Setup(x => x.GenerateQrCode(It.IsAny<string>(), It.IsAny<string>()));
            mock.Setup(x => x.GetQrByName("test_qr")).Returns(Result.Ok(new byte[64]));

            return mock.Object;
        }

        private ILogger<LobbyController> GetLoggerMock()
        {
            var store = new Mock<ILogger<LobbyController>>();
            return store.Object;
        }

        private UserManager<ApplicationUser> GetUserManagerMock(string userId)
        {
            List<ApplicationUser> users = new List<ApplicationUser>()
            {
                new ApplicationUser()
                {
                    Id = userId,
                    UserName = "test",
                    DisplayName = "Test"
                },
            };
            UserManager<ApplicationUser> userManager = MockUserManager<ApplicationUser>(users).Object;

            return userManager;
        }

        private Mock<UserManager<TUser>> MockUserManager<TUser>(List<TUser> ls) where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<TUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

            mgr.Setup(x => x.DeleteAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<TUser, string>((x, y) => ls.Add(x));
            mgr.Setup(x => x.UpdateAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(ls.First());
            mgr.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(ls.First());

            return mgr;
        }
    }
}