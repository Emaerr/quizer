using Microsoft.AspNetCore.Identity;
using Quizer.Models.Quizzes;
using Quizer.Models.User;

namespace Quizer.Services.Quizzes
{
    public interface IQuizService
    {
        public Quiz? GetUserQuizByGuid(ApplicationUser user, string guid);
        public IEnumerable<Quiz> GetUserQuizzes(ApplicationUser user);
        public void Insert(Quiz quiz);
    }
}
