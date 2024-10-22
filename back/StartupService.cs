using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Quizer.Models.User;
using System;

namespace Quizer
{
    public class StartupService : IHostedService
    {
        private IServiceProvider? _serviceProvider;
        private IConfiguration? _configuration;
        public StartupService(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //MigrateDatabase();
            SetUpRoles();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        //private async void MigrateDatabase()
        //{
        //    var scope = _serviceProvider.CreateScope();
        //    var serviceProvider = scope.ServiceProvider;
        //    var migrator = serviceProvider.GetRequiredService<IMigrator>();

        //    await migrator.MigrateAsync();
        //}

        private async void SetUpRoles()
        {
            using var scope = _serviceProvider.CreateScope();

            var RoleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            string[] roleNames = { "Admin", "Member", "Participator" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await RoleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    //create the roles and seed them to the database: Question 1
                    roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            //Here you could create a super user who will maintain the web app
            var superUser = new ApplicationUser
            {
                UserName = _configuration["SuperUser:UserName"],
            };

            var _user = await UserManager.FindByNameAsync(_configuration["SuperUser:UserName"]);

            if (_user != null)
            {
                await UserManager.AddToRoleAsync(_user, "Admin");
            }
        }
    }
}
