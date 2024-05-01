using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Quizer.Models.Quizzes;

namespace Quizer.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Quizer.Models.Quizzes.Quiz> Quizs { get; set; } = default!;

        public DbSet<Quizer.Models.Quizzes.Question> Questions { get; set; } = default!;

        public DbSet<Quizer.Models.Quizzes.Answer> Answers { get; set; } = default!;
    }
}
