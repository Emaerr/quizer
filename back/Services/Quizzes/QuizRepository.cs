using Microsoft.EntityFrameworkCore;
using Quizer.Data;
using Quizer.Models.Quizzes;

namespace Quizer.Services.Quizzes
{
    public class QuizRepository : IQuizRepository
    {
        private QuizContext _context;

        public QuizRepository(QuizContext context)
        {
            _context = context;
        }


        public IEnumerable<Quiz> GetQuizzes()
        {
            return _context.Quizzes.ToList();
        }

        public Quiz? GetQuiz(int id)
        {
            return _context.Quizzes.Find(id);
        }

        public void InsertQuiz(Quiz quiz)
        {
            _context.Quizzes.Add(quiz);
        }

        public void DeleteQuiz(int id)
        {
            Quiz? quiz = _context.Quizzes.Find(id);
            if (quiz != null)
            {
                _context.Quizzes.Remove(quiz);
            }
        }

        public void UpdateQuiz(Quiz quiz)
        {
            _context.Entry(quiz).State = EntityState.Modified;
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public IEnumerable<Quiz> GetUserQuizzes(string userId)
        {
            var quizzes = from q in _context.Quizzes where q.AuthorId == userId select q;
            return quizzes.ToList();
        }
    }
}
