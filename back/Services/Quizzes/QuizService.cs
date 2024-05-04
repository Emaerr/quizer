using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Quizer.Data;
using Quizer.Models.Quizzes;
using Quizer.Models.User;

namespace Quizer.Services.Quizzes
{
    public class QuizService : IQuizService
    { 
        private readonly IServiceScopeFactory _scopeFactory;

        public QuizService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public QuizData? GetUserQuizData(string userId, string guid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            var quizzes = quizRepository.GetUserQuizzes(userId);

            var userQuizzes = from quizz in quizzes where quizz.Guid == guid select quizz;

            if (userQuizzes.IsNullOrEmpty())
            {
                return null;
            } else {
                Quiz quiz = userQuizzes.First();
                return GetDataFromQuiz(quiz);
            }
        }

        public IEnumerable<QuizData> GetUserQuizzesData(string userId)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();

            IEnumerable<Quiz> quizzes = quizRepository.GetUserQuizzes(userId);
            List<QuizData> quizData = [];
            foreach (Quiz quiz in quizzes)
            {
                quizData.Add(GetDataFromQuiz(quiz));
            }
            return quizData;
        }

        public string Create(string authorId)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            Quiz quiz = new Quiz()
            {
                AuthorId = authorId,
                Name = "Unnamed",
                TimeLimit = 15,
            };
            quizRepository.InsertQuiz(quiz);
            quizRepository.Save();
            return quiz.Guid;
        }

        public void Update(QuizData quizData)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            Quiz? quiz = quizRepository.GetQuizByGuid(quizData.Guid);

            if (quiz != null)
            {
                Quiz quizUpdated = GetQuizFromData(quizData);
                quiz.Questions = quizUpdated.Questions;
                quiz.TimeLimit = quizUpdated.TimeLimit;
                quiz.Name = quizUpdated.Name;

                quizRepository.UpdateQuiz(quiz);
                quizRepository.Save();
            }
        }

        public void DeleteUserQuiz(string userId, string guid)
        {
            Quiz? quiz = GetUserQuiz(userId, guid);

            if (quiz != null)
            {
                IServiceScope scope = _scopeFactory.CreateScope();
                IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
                quizRepository.DeleteQuiz(quiz.Id);
                quizRepository.Save();
            }
        }

        private QuizData GetDataFromQuiz(Quiz quiz)
        {   
            List<QuestionData> questionData = [];
            foreach(Question question in quiz.Questions)
            {
                questionData.Add(GetDataFromQuestion(question));
            }

            return new(quiz.Guid, quiz.AuthorId, quiz.Name, quiz.TimeLimit, questionData);
        }

        private QuestionData GetDataFromQuestion(Question question)
        {
            List<AnswerData> answerData = [];
            foreach (Answer answer in question.Answers)
            {
                answerData.Add(new AnswerData(answer.Guid, answer.Title, answer.IsCorrect));
            }

            return new QuestionData(question.Guid, question.Position, question.Title, answerData);
        }

        private Quiz GetQuizFromData(QuizData data)
        {
            List<Question> questions = [];
            foreach(QuestionData qData in data.Questions)
            {
                questions.Add(GetQuestionFromData(qData));
            }

            return new Quiz()
            {
                Guid = data.Guid,
                AuthorId = data.AuthorId,
                Name = data.Name,
                TimeLimit = data.TimeLimit,
                Questions = questions,
            }; ;
        }

        private Question GetQuestionFromData(QuestionData data)
        {
            List<Answer> answers = [];
            foreach(AnswerData aData in data.Answers)
            {
                answers.Add(new Answer()
                {
                    Guid = aData.Guid,
                    Title = aData.Title,
                    IsCorrect = aData.isCorrect
                });
            }

            return new Question()
            {
                Guid = data.Guid,
                Position = data.Position,
                Title = data.Title,
                Answers = answers
            };
        }

        private Quiz? GetUserQuiz(string userId, string guid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            var quizzes = quizRepository.GetUserQuizzes(userId);

            var userQuizzes = from quizz in quizzes where quizz.Guid == guid select quizz;

            if (userQuizzes.IsNullOrEmpty())
            {
                return null;
            }
            else
            {
                Quiz quiz = userQuizzes.First();
                return quiz;
            }
        } 
    }
}
