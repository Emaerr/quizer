using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Quizer.Data;
using Quizer.Models.Quizzes;
using Quizer.Services.Quizzes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quizer.Services.Quizzes.Tests
{
    [TestClass()]
    public class QuestionDataRepositoryTests
    {
        [TestMethod()]
        public void GetUserQuizQuestionsDataTest()
        {
            AppDbContext context = GetContextMock(GenerateTestData());
            QuestionDataRepository repository = new QuestionDataRepository(context);

            IEnumerable<QuestionData> data = repository.GetUserQuizQuestionsData("0", "0");

            QuestionInfo correctInfo = new QuestionInfo(0, "test_question", QuestionType.Test);
            List<AnswerData> correctAnswers = new List<AnswerData>() { new AnswerData("0", new AnswerInfo("test_answer", true)) };
            QuestionData correctData = new QuestionData("0", correctInfo, correctAnswers);

            Assert.AreEqual(data.Count(), 1);
            Assert.AreEqual(data.First().Info.Title, correctData.Info.Title);
            Assert.AreEqual(data.First().Info.Position, correctData.Info.Position);
            Assert.AreEqual(data.First().Answers.Count, correctData.Answers.Count);
            Assert.AreEqual(data.First().Answers.First(), correctData.Answers.First());
        }

        [TestMethod()]
        public void GetUserQuizQuestionDataTest()
        {
            AppDbContext context = GetContextMock(GenerateTestData());
            QuestionDataRepository repository = new QuestionDataRepository(context);

            QuestionData? data = repository.GetUserQuizQuestionData("0", "0", "0");

            QuestionInfo correctInfo = new QuestionInfo(0, "test_question", QuestionType.Test);
            List<AnswerData> correctAnswers = new List<AnswerData>() { new AnswerData("0", new AnswerInfo("test_answer", true)) };
            QuestionData correctData = new QuestionData("0", correctInfo, correctAnswers);

            Assert.IsNotNull(data);
            Assert.AreEqual(data.Info.Title, correctData.Info.Title);
            Assert.AreEqual(data.Info.Position, correctData.Info.Position);
            Assert.AreEqual(data.Answers.Count, correctData.Answers.Count);
            Assert.AreEqual(data.Answers.First(), correctData.Answers.First());
        }

        [TestMethod()]
        public void CreateUserQuizQuestionTest()
        {
            AppDbContext context = GetContextMock(GenerateTestData());
            QuestionDataRepository repository = new QuestionDataRepository(context);

            repository.CreateUserQuizQuestion("0", "0", QuestionType.Test);

            var quizzes = from q in context.Quizzes where q.Guid == "0" select q;

            Assert.AreEqual(quizzes.First().Questions.Count, 2);
        }

        [TestMethod()]
        public void UpdateUserQuizQuestionTest()
        {
            AppDbContext context = GetContextMock(GenerateTestData());
            QuestionDataRepository repository = new QuestionDataRepository(context);

            QuestionInfo updatedInfo = new QuestionInfo(0, "test_question_updated", QuestionType.Test);
            AnswerInfo updatedAnswer = new AnswerInfo("test_answer_updated", true);
            List<AnswerInfo> updatedAnswers = new List<AnswerInfo> { updatedAnswer };
            repository.UpdateUserQuizQuestion("0", "0", "0", updatedInfo, updatedAnswers);

            var quizzes = from q in context.Quizzes where q.Guid == "0" select q;
            Question question = quizzes.First().Questions.First();

            Assert.AreEqual(question.Title, "test_question_updated");
            Assert.AreEqual(question.Answers.First().Title, "test_answer_updated");
        }

        private static AppDbContext GetContextMock(List<Quiz> lstData)
        {
            IQueryable<Quiz> lstDataQueryable = lstData.AsQueryable();
            Mock<DbSet<Quiz>> dbSetMockQuizzes = new Mock<DbSet<Quiz>>();
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseInMemoryDatabase("TestDb");
            Mock<AppDbContext> dbContext = new Mock<AppDbContext>(optionsBuilder.Options);

            dbSetMockQuizzes.As<IQueryable<Quiz>>().Setup(s => s.Provider).Returns(lstDataQueryable.Provider);
            dbSetMockQuizzes.As<IQueryable<Quiz>>().Setup(s => s.Expression).Returns(lstDataQueryable.Expression);
            dbSetMockQuizzes.As<IQueryable<Quiz>>().Setup(s => s.ElementType).Returns(lstDataQueryable.ElementType);
            dbSetMockQuizzes.As<IQueryable<Quiz>>().Setup(s => s.GetEnumerator()).Returns(() => lstDataQueryable.GetEnumerator());
            dbSetMockQuizzes.Setup(x => x.Add(It.IsAny<Quiz>())).Callback<Quiz>(lstData.Add);
            dbSetMockQuizzes.Setup(x => x.AddRange(It.IsAny<IEnumerable<Quiz>>())).Callback<IEnumerable<Quiz>>(lstData.AddRange);
            dbSetMockQuizzes.Setup(x => x.Remove(It.IsAny<Quiz>())).Callback<Quiz>(t => lstData.Remove(t));
            dbSetMockQuizzes.Setup(x => x.RemoveRange(It.IsAny<IEnumerable<Quiz>>())).Callback<IEnumerable<Quiz>>(ts =>
            {
                foreach (var t in ts) { lstData.Remove(t); }
            });


            dbContext.Setup(c => c.Quizzes).Returns(dbSetMockQuizzes.Object);

            return dbContext.Object;
        }

        private static List<Quiz> GenerateTestData()
        {
            List<Quiz> list = [];
            Quiz quiz = new Quiz() { AuthorId = "0", Guid = "0", Id = 0, Name = "test_quiz", TimeLimit = 15 };
            list.Add(quiz);

            Answer answer = new Answer() { Guid = "0", Id = 0, IsCorrect = true, Title = "test_answer" };

            Question question = new Question()
            {
                Title = "test_question",
                Guid = "0",
                Id = 0,
                Position = 0,
                Answers = new List<Answer>() { answer },
                Type = QuestionType.Test
            };

            quiz.Questions.Add(question);


            return list;
        }
    }
}