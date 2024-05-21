using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Quizer.Data;
using Quizer.Models.Quizzes;
using Quizer.Models.User;
using Quizer.Services.Util;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Quizer.Services.Quizzes
{
    public class QuizDataRepository : IQuizDataRepository
    { 
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<QuizDataRepository> _logger;

        public QuizDataRepository(IServiceScopeFactory scopeFactory, ILogger<QuizDataRepository> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
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

            _logger.LogInformation(ServiceLogEvents.QuizCreated, "Succesfully created quiz {quizGuid} for user {authorId}", quiz.Guid, authorId);

            return quiz.Guid;
        }

        public void UpdateUserQuizInfo(string userId, string guid, QuizInfo quizInfo)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            Quiz? quiz = quizRepository.GetQuizByGuid(guid);

            if (quiz == null)
            {
                _logger.LogInformation(ServiceLogEvents.QuizUpdateError, "Couldn't update quiz {guid} because the quiz with that GUID not found", guid);
                return;
            }

            if (quiz.AuthorId != userId)
            {
                _logger.LogInformation(ServiceLogEvents.QuizUpdateError, "User {userId} tried to update quiz {guid} but he is not the author", userId, guid);
                return;
            }

            quiz.TimeLimit = quizInfo.TimeLimit;
            quiz.Name = quizInfo.Name;

            quizRepository.UpdateQuiz(quiz);
            quizRepository.Save();

            _logger.LogInformation(ServiceLogEvents.QuizUpdated, "Succesfully updated quiz {quizGuid}", guid);
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
                _logger.LogInformation(ServiceLogEvents.QuizDeleted, "Succesfully deleted quiz {guid} for user {userId}", guid, userId);
            } else
            {
                _logger.LogInformation(ServiceLogEvents.QuizDeletionError, "Couldn't delete quiz {guid} because quiz with that GUID not found", guid);
            }
        }

        private QuizData GetDataFromQuiz(Quiz quiz)
        {   
            return new QuizData(quiz.Guid, new QuizInfo(quiz.Name, quiz.TimeLimit));
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
