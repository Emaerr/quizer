namespace Quizer.Services.Quizzes
{
    public interface IQuestionDataRepository
    {
        public QuestionData? GetUserQuizQuestionData(string userId, string quizGuid, string questionGuid);
        public string? CreateUserQuizQuestion(string userId, string quizGuid);
        public void UpdateUserQuizQuestion(string userId, string quizGuid, string questionGuid, QuestionInfo info, List<AnswerInfo> answers);
        public void DeleteUserQuizQuestion(string userId, string quizGuid, string questionGuid);
    }

    public record QuestionInfo(int Position, string Title);
    public record AnswerInfo(string Title, bool IsCorrect);
    public record AnswerData(string Guid, AnswerInfo Info);
    public record QuestionData(QuestionInfo Info, List<AnswerData> Answers);
    
}
