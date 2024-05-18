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
using QuizerTests.Services.Lobbies.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quizer.Services.Lobbies.impl.Tests
{
    [TestClass()]
    public class LobbyControlServiceTests
    {


        [TestMethod()]
        public async Task CreateAsyncTestAsync()
        {
            ILobbyControlService service = new LobbyControlService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyRepositoryMock()), new TestTimeService(), LobbyMocks.GetLoggerMock());
            Result<string> result = await service.CreateAsync("0", "0", 10);
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public void ForceNextQuestionAsyncTestAsync()
        {
            ILobbyControlService service = new LobbyControlService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyRepositoryMock()), new TestTimeService(), LobbyMocks.GetLoggerMock());
            Result result = service.ForceNextQuestionAsync("0");
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public async Task JoinUserAsyncTestAsync()
        {
            ILobbyControlService service = new LobbyControlService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyRepositoryMock()), new TestTimeService(), LobbyMocks.GetLoggerMock());
            Result<string> result = await service.JoinUserAsync("0", "1");
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public async Task KickUserAsyncTestAsync()
        {
            ILobbyControlService service = new LobbyControlService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyRepositoryMock()), new TestTimeService(), LobbyMocks.GetLoggerMock());
            Result<string> result = await service.KickUserAsync("0", "1");
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public async Task StartLobbyAsyncTestAsync()
        {
            ILobbyControlService service = new LobbyControlService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyRepositoryMock()), new TestTimeService(), LobbyMocks.GetLoggerMock());
            Result<string> result = await service.StartLobbyAsync("0");
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod()]
        public async Task StopLobbyAsyncTestAsync()
        {
            ILobbyControlService service = new LobbyControlService(LobbyMocks.GetScopeFactoryMock(LobbyMocks.GetLobbyRepositoryMock()), new TestTimeService(), LobbyMocks.GetLoggerMock());
            Result<string> result = await service.StopLobbyAsync("0");
            Assert.IsTrue(result.IsSuccess);
        }

        //[TestMethod()]
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
    }
}