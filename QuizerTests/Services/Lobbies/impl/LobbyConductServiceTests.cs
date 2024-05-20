using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;
using Quizer.Models.User;
using Quizer.Services.Lobbies.impl;
using Quizer.Services.Quizzes;
using Quizer.Services.Util;
using QuizerTests.Services.Lobbies.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quizer.Services.Lobbies.impl.Tests
{
    [TestClass()]
    public class LobbyConductServiceTests
    {
        [TestMethod()]
        public void GetLobbyStatusTest()
        {

        }

        [TestMethod()]
        public async Task GetCurrentQuestionTestAsync()
        {
            ILobbyConductService service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyRepositoryMock(isLobbyStarted: false)), new TestTimeService(), LobbyMocks.GetLoggerMock());
            Result<QuestionData> result = service.GetCurrentQuestion("0");
            Assert.IsTrue(result.IsFailed);

            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyRepositoryMock(isLobbyStarted: true)), new TestTimeService(), LobbyMocks.GetLoggerMock());
            result = service.GetCurrentQuestion("0");
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public async Task RegisterTestAnswerTestAsync()
        {
            // check for invalid user
            ILobbyConductService service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyRepositoryMock(isLobbyStarted: false)), new TestTimeService(), LobbyMocks.GetLoggerMock());
            Result<QuestionData> result = await service.RegisterTestAnswer("0", "0", "0");
            Assert.IsTrue(result.IsFailed);

            // check for invalid answer guid
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: false)), new TestTimeService(), LobbyMocks.GetLoggerMock());
            result = await service.RegisterTestAnswer("0", "0", "invalid_guid");
            Assert.IsTrue(result.IsFailed);

            // check for lobby not started
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: false)), new TestTimeService(), LobbyMocks.GetLoggerMock());
            result = await service.RegisterTestAnswer("0", "0", "0");
            Assert.IsTrue(result.IsFailed);

            // should be success
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: true)), new TestTimeService(), LobbyMocks.GetLoggerMock());
            result = await service.RegisterTestAnswer("0", "0", "0");
            Assert.IsTrue(result.IsSuccess);
        }
    }
}