using Quizer.Models.Quizzes;

namespace Quizer.Services.Quizzes
{
    public interface IQuizRepository
    {
        public IEnumerable<Quiz> GetQuizzes();

        public Quiz? GetQuiz(int id);

        public void InsertQuiz(Quiz quiz);

        public void DeleteQuiz(int id);

        public void UpdateQuiz(Quiz quiz);

        public void Save();
    }
}
