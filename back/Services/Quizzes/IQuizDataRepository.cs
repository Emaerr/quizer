using Microsoft.AspNetCore.Identity;
using Quizer.Models.Quizzes;
using Quizer.Models.User;

namespace Quizer.Services.Quizzes
{
    public interface IQuizDataRepository
    {
        public QuizData? GetUserQuizData(string userId, string guid);
        public IEnumerable<QuizData> GetUserQuizzesData(string userId);
        public string Create(string authorId);
        public void UpdateUserQuizInfo(string userId, string quizGuid, QuizInfo quizInfo);
        public void DeleteUserQuiz(string userId, string guid);
    }

    public record QuizInfo(string Name, int TimeLimit);
    public record QuizData(string Guid, QuizInfo Info);
}
