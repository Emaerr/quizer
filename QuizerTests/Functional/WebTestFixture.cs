using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.EntityFrameworkCore.Sqlite;
using Quizer.Data;
using Quizer.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace QuizerTests.Functional
{
    public class WebTestFixture : WebApplicationFactory<Quizer.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.AddEntityFrameworkInMemoryDatabase();

                // Create a new service provider.
                var provider = services
                      .AddEntityFrameworkInMemoryDatabase()
                      .BuildServiceProvider();

                DbConnection _connection = new SqliteConnection("Filename=:memory:");
                _connection.Open();

                // Add a database context (ApplicationDbContext) using an in-memory
                // database for testing.
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                    options.UseInternalServiceProvider(provider);
                });

                //services.AddDbContext<AppIdentityDbContext>(options =>
                //{
                //    options.UseInMemoryDatabase("Identity");
                //    options.UseInternalServiceProvider(provider);
                //});

                // Build the service provider.
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database
                // context (ApplicationDbContext).
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<AppDbContext>();
                    var loggerFactory = scopedServices.GetRequiredService<ILoggerFactory>();

                    var logger = scopedServices
                        .GetRequiredService<ILogger<WebTestFixture>>();

                    // Ensure the database is created.
                    db.Database.EnsureCreated();

                    //try
                    //{
                    //    // Seed the database with test data.
                    //    AppDbContextSeed.SeedAsync(db, logger).Wait();

                    //    // seed sample user data
                    //    var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
                    //    var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
                    //    //AppDbContextSeed.SeedAsync(userManager, roleManager).Wait();
                    //}
                    //catch (Exception ex)
                    //{
                    //    logger.LogError(ex, $"An error occurred seeding the " +
                    //              $"database with test messages. Error: {ex.Message}");
                    //}
                }
            });
        }
    }
}
