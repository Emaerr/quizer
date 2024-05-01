using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Quizer.Models.Quizzes;

namespace Quizer.Data
{
    public class LobbyContext : IdentityDbContext
    {
        public LobbyContext(DbContextOptions<IdentityContext> options)
    : base(options)
        {
        }

        public DbSet<Quizer.Models.Lobbies.Lobby> Lobbies { get; set; } = default!;
    }
}
