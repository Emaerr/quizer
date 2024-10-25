using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Quizer.Data;
using Quizer.Models.Quizzes;
using Quizer.Services.Quizzes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Quizer.Services.Quizzes.Tests
{
    [TestClass()]
    public class QuizRepositoryTests
    {
        private static IEnumerable<string> _usedIds = new List<string>() {
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
            };

        public QuizRepositoryTests()
        {

        }

        [TestMethod()]
        public void GetUserQuizzesTest()
        {
            AppDbContext context = GetContextMock(GenerateTestData());
            QuizRepository repo = new QuizRepository(context);

            IEnumerable<Quiz> quizzes = repo.GetUserQuizzes("8");

            Assert.AreEqual(quizzes.Count(), 1);
            Assert.AreEqual(quizzes.First().AuthorId, "8");
        }

        [TestMethod()]
        public void InsertQuizTest()
        {
            AppDbContext context = GetContextMock(new List<Quiz>());
            QuizRepository repo = new QuizRepository(context);

            repo.InsertQuiz(new Quiz() {Id=0, Guid = "0" });

            var quizzes = from q in context.Quizzes where q.Id == 0 select q;
            Quiz? quiz = quizzes.First();

            Assert.AreEqual(context.Quizzes.Count(), 1);
            Assert.IsNotNull(quiz);
            Assert.AreEqual(quiz.Guid, "0");
        }

        private static AppDbContext GetContextMock(List<Quiz> lstData)
        {
            IQueryable<Quiz> lstDataQueryable = lstData.AsQueryable();
            Mock<DbSet<Quiz>> dbSetMock = new Mock<DbSet<Quiz>>();
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseInMemoryDatabase("TestDb");
            Mock<AppDbContext> dbContext = new Mock<AppDbContext>(optionsBuilder.Options);

            dbSetMock.As<IQueryable<Quiz>>().Setup(s => s.Provider).Returns(lstDataQueryable.Provider);
            dbSetMock.As<IQueryable<Quiz>>().Setup(s => s.Expression).Returns(lstDataQueryable.Expression);
            dbSetMock.As<IQueryable<Quiz>>().Setup(s => s.ElementType).Returns(lstDataQueryable.ElementType);
            dbSetMock.As<IQueryable<Quiz>>().Setup(s => s.GetEnumerator()).Returns(() => lstDataQueryable.GetEnumerator());
            dbSetMock.Setup(x => x.Add(It.IsAny<Quiz>())).Callback<Quiz>(lstData.Add);
            dbSetMock.Setup(x => x.AddRange(It.IsAny<IEnumerable<Quiz>>())).Callback<IEnumerable<Quiz>>(lstData.AddRange);
            dbSetMock.Setup(x => x.Remove(It.IsAny<Quiz>())).Callback<Quiz>(t => lstData.Remove(t));
            dbSetMock.Setup(x => x.RemoveRange(It.IsAny<IEnumerable<Quiz>>())).Callback<IEnumerable<Quiz>>(ts =>
            {
                foreach (var t in ts) { lstData.Remove(t); }
            });


            dbContext.Setup(c => c.Quizzes).Returns(dbSetMock.Object);

            return dbContext.Object;
        }

        private static List<Quiz> GenerateTestData()
        {
            List<Quiz> list = [];
            int i = 0;
            foreach (string id in _usedIds)
            {
                list.Add(new Quiz() { AuthorId = id, Guid = Guid.NewGuid().ToString(), Id = i, Name = "test_quiz_" + id, TimeLimit = 15 });
                i++;
            }

            return list;
        }

    }
}