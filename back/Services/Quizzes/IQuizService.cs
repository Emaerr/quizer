using Microsoft.AspNetCore.Identity;
using Quizer.Models.Quizzes;
using Quizer.Models.User;

namespace Quizer.Services.Quizzes
{
    public interface IQuizService
    {
        public QuizData? GetUserQuizData(string userId, string guid);
        public IEnumerable<QuizData> GetUserQuizzesData(string userId);
        public string Create(string authorId);
        public void Update(QuizData quizData);
        public void DeleteUserQuiz(string userId, string guid);
    }

    public record AnswerData(string Guid, string Title, bool isCorrect);
    public record QuestionData(string Guid, int Position, string Title, List<AnswerData> Answers);
    public record QuizData(string Guid, string AuthorId, string Name, int TimeLimit, List<QuestionData> Questions);
}
