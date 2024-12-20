using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Pomelo.EntityFrameworkCore.MySql.Migrations.Internal;
using Quizer.Data;
using Quizer.Hubs;
using Quizer.Models.User;
using Quizer.Services.Lobbies;
using Quizer.Services.Lobbies.impl;
using Quizer.Services.Qr;
using Quizer.Services.Quizzes;
using Quizer.Services.Util;


namespace Quizer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builderConf = WebApplication.CreateBuilder(args);

            string? webRootPath = builderConf.Configuration["WebRootPath"];

            string? appDirectory = builderConf.Configuration["AppDirectory"] ?? Directory.GetCurrentDirectory();

            WebApplicationOptions webApplicationOptions = new()
            {
                ApplicationName = typeof(Program).Assembly.FullName,
                ContentRootPath = appDirectory,
                EnvironmentName = builderConf.Environment.EnvironmentName,
                WebRootPath = webRootPath ?? Path.Combine(appDirectory, "wwwroot"),
                Args = args
            };
            var builder = WebApplication.CreateBuilder(webApplicationOptions);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            
            ServerVersion? version = null;
            try
            {
                version = ServerVersion.AutoDetect(connectionString);
            } catch (MySqlConnector.MySqlException)
            {
                ServerVersion.TryParse(builder.Configuration["SqlServerVersion"], out version);
            }

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connectionString, version)); //ServerVersion.AutoDetect(connectionString)
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddSignalR();

            builder.Services.AddDefaultIdentity<ApplicationUser>()
            .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<IHostedService, StartupService>();
            builder.Services.AddScoped<IQuizRepository, QuizRepository>();
            builder.Services.AddScoped<ILobbyRepository, LobbyRepository>();
            builder.Services.AddScoped<IQuizDataRepository, QuizDataRepository>();
            builder.Services.AddScoped<IQuestionDataRepository, QuestionDataRepository>();
            builder.Services.AddScoped<IParticipatorRepository, ParticipatorRepository>();
            builder.Services.AddSingleton<ILobbyConductService, LobbyConductService>();
            builder.Services.AddSingleton<ILobbyControlService, LobbyControlService>();
            builder.Services.AddSingleton<ILobbyAuthService, LobbyAuthService>();
            builder.Services.AddSingleton<ILobbyStatsService, LobbyStatsService>();
            builder.Services.AddSingleton<LobbyService>();
            builder.Services.AddSingleton<IHostedService>(x => x.GetRequiredService<LobbyService>());
            builder.Services.AddSingleton<ILobbyUpdateService, LobbyService>(x => x.GetRequiredService<LobbyService>());
            builder.Services.AddSingleton<ITimeService, TimeService>();
            builder.Services.AddSingleton<IQrService, QrService>();
            builder.Services.AddSingleton<ITempUserService, TempUserService>();

            builder.Services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("ParticipatorRights", policy => policy.RequireRole("Participator", "Member", "Admin"));
                options.AddPolicy("MemberRights", policy => policy.RequireRole("Member", "Admin"));
                options.AddPolicy("AdminRights", policy => policy.RequireRole("Admin"));
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            StaticFileOptions options = new StaticFileOptions()
            {
                RequestPath = builder.Configuration["StaticFileRequestPath"] ?? "",
            };
            app.UseStaticFiles(options);

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<LobbyHub>("/LobbyHub");

            app.MapRazorPages();

            app.Run();
            
        }

    }
}
