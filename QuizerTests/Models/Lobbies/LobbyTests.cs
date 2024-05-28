using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quizer.Models.Lobbies.Tests
{
    [TestClass()]
    public class LobbyTests
    {
        [TestMethod()]
        public void UpdateTest()
        {
            Lobby lobby = new Lobby()
            {
                Quiz = new Quiz()
                {
                    TimeLimit = 10000,
                    BreakTime = 5000,
                    Questions = new List<Question> { 
                        new Question()
                        {
                            Position = 0,
                        },
                        new Question()
                        {
                            Position = 1,
                        }
                    }
                },
                IsStarted = true,
            };

            lobby.Update(new TimeSpan(0, 0, 0));
            Question? q1 = lobby.GetCurrentQuestion();
            Assert.IsNotNull(q1);
            Assert.IsTrue(lobby.IsQuestionTime());
            Assert.AreEqual(q1.Position, 0);

            lobby.Update(new TimeSpan(0, 0, 9));
            Question? q2 = lobby.GetCurrentQuestion();
            Assert.IsNotNull(q2);
            Assert.IsTrue(lobby.IsQuestionTime());
            Assert.AreEqual(q2.Position, 0);

            lobby.Update(new TimeSpan(0, 0, 0, 1, 500));
            Question? q3 = lobby.GetCurrentQuestion();
            Assert.IsNotNull(q3);
            Assert.IsTrue(lobby.IsAnsweringTime());
            Assert.AreEqual(q3.Position, 0);

            lobby.Update(new TimeSpan(0, 0, 1));
            Question? q4 = lobby.GetCurrentQuestion();
            Assert.IsNotNull(q4);
            Assert.IsTrue(lobby.IsBreakTime());
            Assert.AreEqual(q4.Position, 0);

            lobby.Update(new TimeSpan(0, 0, 5));
            Question? q5 = lobby.GetCurrentQuestion();
            Assert.IsNotNull(q5);
            Assert.IsTrue(lobby.IsQuestionTime());
            Assert.AreEqual(q5.Position, 1);
        }
    }
}