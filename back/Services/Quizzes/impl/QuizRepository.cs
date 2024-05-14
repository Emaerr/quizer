using Microsoft.EntityFrameworkCore;
using Quizer.Data;
using Quizer.Models.Quizzes;
using System.Data;

namespace Quizer.Services.Quizzes
{
    public class QuizRepository : IQuizRepository
    {
        private AppDbContext _context;

        public QuizRepository(AppDbContext context)
        {
            _context = context;
        }


        public IEnumerable<Quiz> GetQuizzes()
        {
            return _context.Quizzes.ToList();
        }

        public Quiz? GetQuizById(int id)
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
            _context.Update(quiz);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public IEnumerable<Quiz> GetUserQuizzes(string userId)
        {
            var quizzes = from q in _context.Quizzes where q.AuthorId == userId select q;
            return quizzes.ToList();
        }

        public Quiz? GetQuizByGuid(string guid)
        {
            var quizzes = from q in _context.Quizzes where q.Guid == guid select q;
            return quizzes.FirstOrDefault();
        }
    }
}
