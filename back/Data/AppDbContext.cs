using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;
using Quizer.Models.User;

namespace Quizer.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        public DbSet<Participator> Participators { get; set; } = default!;
        public DbSet<Quiz> Quizzes { get; set; } = default!;
        public DbSet<Question> Questions { get; set; } = default!;
        public DbSet<Answer> Answers { get; set; } = default!;
        public DbSet<Lobby> Lobbies { get; set; } = default!;

        //#region Required
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Lobby>().HasOne(l => l.Quiz).WithMany();
        //}
        //#endregion
    }
}
