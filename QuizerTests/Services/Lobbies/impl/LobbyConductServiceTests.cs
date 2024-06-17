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
        public void GetCurrentQuestionTest()
        {
            ILobbyConductService service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyRepositoryMock(isLobbyStarted: false)), LobbyMocks.GetLoggerMock<LobbyConductService>());
            Result<Question> result = service.GetCurrentQuestion("0");
            Assert.IsTrue(result.IsFailed);

            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyRepositoryMock(isLobbyStarted: true)), LobbyMocks.GetLoggerMock<LobbyConductService>());
            result = service.GetCurrentQuestion("0");
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public async Task RegisterTestAnswerTestAsync()
        {
            // check for invalid user
            ILobbyConductService service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyRepositoryMock(isLobbyStarted: false)), LobbyMocks.GetLoggerMock<LobbyConductService>());
            Result result = await service.RegisterTestAnswer("0", "0", "0");
            Assert.IsTrue(result.IsFailed);

            // check for invalid time
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: false, LobbyStage.Question)), LobbyMocks.GetLoggerMock<LobbyConductService>());
            result = await service.RegisterTestAnswer("0", "0", "0");
            Assert.IsTrue(result.IsFailed);

            // check for invalid answer guid
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: false, LobbyStage.Answering)), LobbyMocks.GetLoggerMock<LobbyConductService>());
            result = await service.RegisterTestAnswer("0", "0", "invalid_guid");
            Assert.IsTrue(result.IsFailed);

            // check for lobby not started
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: false, LobbyStage.Answering)), LobbyMocks.GetLoggerMock<LobbyConductService>());
            result = await service.RegisterTestAnswer("0", "0", "0");
            Assert.IsTrue(result.IsFailed);

            // check for answer duplication
            ILobbyRepository lobbyRepository = LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: true, LobbyStage.Answering, questionType: QuestionType.NumberEntry);
            lobbyRepository.GetLobbyByGuid("0")!.NextQuestion();
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(lobbyRepository), LobbyMocks.GetLoggerMock<LobbyConductService>());
            Assert.IsTrue(result.IsFailed);

            // should be success
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: true, LobbyStage.Answering)), LobbyMocks.GetLoggerMock<LobbyConductService>());
            result = await service.RegisterTestAnswer("0", "0", null);
            Assert.IsTrue(result.IsSuccess);

            // should be success
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: true, LobbyStage.Answering)), LobbyMocks.GetLoggerMock<LobbyConductService>());
            result = await service.RegisterTestAnswer("0", "0", "0");
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public async Task RegisterNumericalAnswerTest()
        {
            // check for invalid user
            ILobbyConductService service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyRepositoryMock(isLobbyStarted: false, questionType: QuestionType.NumberEntry)), LobbyMocks.GetLoggerMock<LobbyConductService>());
            Result result = await service.RegisterNumericalAnswer("0", "0", 0);
            Assert.IsTrue(result.IsFailed);

            // check for invalid time
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: false, LobbyStage.Question, questionType: QuestionType.NumberEntry)), LobbyMocks.GetLoggerMock<LobbyConductService>());
            result = await service.RegisterNumericalAnswer("0", "0", 0);
            Assert.IsTrue(result.IsFailed);

            // check for lobby not started
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: false, LobbyStage.Answering, questionType: QuestionType.NumberEntry)), LobbyMocks.GetLoggerMock<LobbyConductService>());
            result = await service.RegisterNumericalAnswer("0", "0", 0);
            Assert.IsTrue(result.IsFailed);

            // check for answer duplication
            ILobbyRepository lobbyRepository = LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: true, LobbyStage.Answering, questionType: QuestionType.NumberEntry);
            lobbyRepository.GetLobbyByGuid("0")!.NextQuestion();
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(lobbyRepository), LobbyMocks.GetLoggerMock<LobbyConductService>());
            result = await service.RegisterNumericalAnswer("0", "0", 0);
            Assert.IsTrue(result.IsFailed);

            // should be success
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: true, LobbyStage.Answering, questionType: QuestionType.NumberEntry)), LobbyMocks.GetLoggerMock<LobbyConductService>());
            result = await service.RegisterNumericalAnswer("0", "0", null);
            Assert.IsTrue(result.IsSuccess);

            // should be success
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: true, LobbyStage.Answering, questionType: QuestionType.NumberEntry)), LobbyMocks.GetLoggerMock<LobbyConductService>());
            result = await service.RegisterNumericalAnswer("0", "0", 0);
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public async Task RegisterTextAnswerTest()
        {
            // check for invalid user
            ILobbyConductService service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyRepositoryMock(isLobbyStarted: false, questionType: QuestionType.TextEntry)), LobbyMocks.GetLoggerMock<LobbyConductService>());
            Result result = await service.RegisterTextAnswer("0", "0", "test");
            Assert.IsTrue(result.IsFailed);

            // check for invalid time
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: false, LobbyStage.Question, questionType: QuestionType.TextEntry)), LobbyMocks.GetLoggerMock<LobbyConductService>());
            result = await service.RegisterTextAnswer("0", "0", "test");
            Assert.IsTrue(result.IsFailed);

            // check for lobby not started
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: false, LobbyStage.Answering, questionType: QuestionType.TextEntry)), LobbyMocks.GetLoggerMock<LobbyConductService>());
            result = await service.RegisterTextAnswer("0", "0", "test");
            Assert.IsTrue(result.IsFailed);

            // check for answer duplication
            ILobbyRepository lobbyRepository = LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: true, LobbyStage.Answering, questionType: QuestionType.NumberEntry);
            lobbyRepository.GetLobbyByGuid("0")!.NextQuestion();
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(lobbyRepository), LobbyMocks.GetLoggerMock<LobbyConductService>());
            result = await service.RegisterTextAnswer("0", "0", "test");
            Assert.IsTrue(result.IsFailed);

            // should be success
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: true, LobbyStage.Answering, questionType: QuestionType.TextEntry)), LobbyMocks.GetLoggerMock<LobbyConductService>());
            result = await service.RegisterTextAnswer("0", "0", null);
            Assert.IsTrue(result.IsSuccess);

            // should be success
            service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: true, LobbyStage.Answering, questionType: QuestionType.TextEntry)), LobbyMocks.GetLoggerMock<LobbyConductService>());
            result = await service.RegisterTextAnswer("0", "0", "test");
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public void SubsribeToLobbyStatusUpdateEventTest()
        {
            bool isEventRaised = false;
            ILobbyRepository lobbyRepository = LobbyMocks.GetLobbyWithUserRepositoryMock(isLobbyStarted: true, LobbyStage.Answering, questionType: QuestionType.NumberEntry);
            var service = new LobbyConductService(LobbyMocks.GetScopeFactoryMock(lobbyRepository), LobbyMocks.GetLoggerMock<LobbyConductService>());
            service.SubscribeToLobbyStatusUpdateEvent("0", (status) => isEventRaised = true);
            lobbyRepository.GetLobbyByGuid("0")!.NextQuestion();

            Assert.IsTrue(isEventRaised);
        }
    }
}