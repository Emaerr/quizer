using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Quizer.Models.Lobbies;
using Quizer.Models.Quizzes;
using System.Reflection.Metadata;

namespace Quizer.Data
{
    public class LobbyContext : DbContext
    {
        public LobbyContext(DbContextOptions<LobbyContext> options)
    : base(options)
        {
        }

        public DbSet<Lobby> Lobbies { get; set; } = default!;
    }
}
