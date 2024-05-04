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

        public Quiz? GetUserQuiz(ApplicationUser user, string guid)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            var quizzes = quizRepository.GetUserQuizzes(user.Id);

            var userQuizzes = from quizz in quizzes where quizz.Guid == guid select quizz;

            if (userQuizzes.IsNullOrEmpty())
            {
                return null;
            } else { 
                return userQuizzes.First();
            }
        }

        public IEnumerable<Quiz> GetUserQuizzes(ApplicationUser user)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            return quizRepository.GetUserQuizzes(user.Id);
        }

        public void Insert(Quiz quiz)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            quizRepository.InsertQuiz(quiz);
            quizRepository.Save();
        }

        public void Update(Quiz quiz)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            quizRepository.UpdateQuiz(quiz);
            quizRepository.Save();
        }

        public void DeleteUserQuiz(ApplicationUser user, string guid)
        {
            Quiz? quiz = GetUserQuiz(user, guid);

            if (quiz != null)
            {
                IServiceScope scope = _scopeFactory.CreateScope();
                IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
                quizRepository.DeleteQuiz(quiz.Id);
                quizRepository.Save();
            }
        }
    }
}
