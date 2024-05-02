using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;

namespace Quizer.Data
{
    public class QuizContext : DbContext { 

        public QuizContext(DbContextOptions<QuizContext> options)
    : base(options)
        {
        }
        public DbSet<Quiz> Quizzes { get; set; } = default!;

        public DbSet<Question> Questions { get; set; } = default!;

        public DbSet<Answer> Answers { get; set; } = default!;
    }
}
