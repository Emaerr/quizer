using Quizer.Models.Quizzes;
using System.Composition.Convention;

namespace Quizer.Services.Quizzes
{
    public interface IQuestionDataRepository
    {
        public IEnumerable<QuestionData> GetUserQuizQuestionsData(string userId, string quizGuid);
        public QuestionData? GetUserQuizQuestionData(string userId, string quizGuid, string questionGuid);
        public string? CreateUserQuizQuestion(string userId, string quizGuid, QuestionType type);
        public void UpdateUserQuizQuestion(string userId, string quizGuid, string questionGuid, QuestionInfo info, List<AnswerInfo> answers);
        public void DeleteUserQuizQuestion(string userId, string quizGuid, string questionGuid);
    }

    public record QuestionInfo(int Position, string Title, QuestionType Type);
    public record AnswerInfo(string Title, bool IsCorrect);
    public record AnswerData(string Guid, AnswerInfo Info);
    public record QuestionData(string Guid, QuestionInfo Info, List<AnswerData> Answers);
    
}
