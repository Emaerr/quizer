using Microsoft.AspNetCore.Identity;
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
            using var scope = _serviceProvider.CreateScope();

            var RoleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            string[] roleNames = { "Admin", "Manager", "Member" };
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
                Email = _configuration["SuperUser:Email"],
            };
            //Ensure you have these values in your appsettings.json file
            string userPWD = _configuration["SuperUser:Password"];
            var _user = await UserManager.FindByEmailAsync(_configuration["SuperUser:Email"]);

            if (_user == null)
            {
                var createPowerUser = await UserManager.CreateAsync(superUser, userPWD);
                if (createPowerUser.Succeeded)
                {
                    //here we tie the new user to the role
                    await UserManager.AddToRoleAsync(superUser, "Admin");

                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
