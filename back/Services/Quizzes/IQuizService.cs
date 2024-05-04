using Microsoft.AspNetCore.Identity;
using Quizer.Models.Quizzes;
using Quizer.Models.User;

namespace Quizer.Services.Quizzes
{
    public interface IQuizService
    {
        public Quiz? GetUserQuiz(ApplicationUser user, string guid);
        public IEnumerable<Quiz> GetUserQuizzes(ApplicationUser user);
        public void Insert(Quiz quiz);
        public void Update(Quiz quiz);
        public void DeleteUserQuiz(ApplicationUser user, string guid);
    }
}
