using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Quizer.Data;
using Quizer.Models.Quizzes;

namespace Quizer.Services.Quizzes
{
    public class QuizService : IQuizService
    { 
        private readonly IServiceScopeFactory _scopeFactory;

        public QuizService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Quiz? GetUserQuiz(IdentityUser user, int quizId)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            var quizzes = quizRepository.GetUserQuizzes(user.Id);

            var userQuizzes = from quizz in quizzes where quizz.Id == quizId select quizz;

            if (userQuizzes.IsNullOrEmpty())
            {
                return null;
            } else { 
                return userQuizzes.First();
            }
        }

        public IEnumerable<Quiz> GetUserQuizzes(IdentityUser user)
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            IQuizRepository quizRepository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
            return quizRepository.GetUserQuizzes(user.Id);
        }
    }
}
