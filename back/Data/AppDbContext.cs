﻿using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;
using Quizer.Models.User;

namespace Quizer.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            
        }

        public virtual DbSet<Participator> Participators { get; set; } = default!;
        public virtual DbSet<Quiz> Quizzes { get; set; } = default!;
        public virtual DbSet<Question> Questions { get; set; } = default!;
        public virtual DbSet<Answer> Answers { get; set; } = default!;
        public virtual DbSet<Lobby> Lobbies { get; set; } = default!;
        public virtual DbSet<ParticipatorAnswer> ParticipatorAnswers { get; set; } = default!;
        public virtual DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        #region Required
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Quiz>().Navigation(q => q.Questions).AutoInclude();
            modelBuilder.Entity<Question>().Navigation(q => q.Answers).AutoInclude();
            modelBuilder.Entity<Lobby>().Navigation(q => q.Participators).AutoInclude();
            modelBuilder.Entity<Lobby>().Navigation(q => q.Quiz).AutoInclude();
            modelBuilder.Entity<Participator>().Navigation(q => q.Answers).AutoInclude();
            modelBuilder.Entity<ParticipatorAnswer>().Navigation(q => q.TestAnswer).AutoInclude();
            modelBuilder.Entity<ParticipatorAnswer>().Navigation(q => q.Question).AutoInclude();

            base.OnModelCreating(modelBuilder);
        }
        #endregion
    }
}
