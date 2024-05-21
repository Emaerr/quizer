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
                    TimeLimit = 10,
                    BreakTime = 5,
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
            Assert.IsTrue(!lobby.IsBreakTime());
            Assert.AreEqual(q1.Position, 0);

            lobby.Update(new TimeSpan(0, 0, 9));
            Question? q2 = lobby.GetCurrentQuestion();
            Assert.IsNotNull(q2);
            Assert.IsTrue(!lobby.IsBreakTime());
            Assert.AreEqual(q2.Position, 0);

            lobby.Update(new TimeSpan(0, 0, 12));
            Question? q3 = lobby.GetCurrentQuestion();
            Assert.IsNotNull(q3);
            Assert.IsTrue(lobby.IsBreakTime());
            Assert.AreEqual(q3.Position, 0);

            lobby.Update(new TimeSpan(0, 0, 16));
            Question? q4 = lobby.GetCurrentQuestion();
            Assert.IsNotNull(q4);
            Assert.IsTrue(!lobby.IsBreakTime());
            Assert.AreEqual(q4.Position, 1);
        }
    }
}