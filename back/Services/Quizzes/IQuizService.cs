using Microsoft.AspNetCore.Identity;
using Quizer.Models.Quizzes;

namespace Quizer.Services.Quizzes
{
    public interface IQuizService
    {
        public Quiz? GetUserQuiz(IdentityUser user, int quizId);
        public IEnumerable<Quiz> GetUserQuizzes(IdentityUser user);
    }
}
