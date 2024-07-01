using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Quizer.Controllers;
using Quizer.Models.User;
using Quizer.Services.Lobbies.impl;
using Quizer.Services.Lobbies;
using Quizer.Services.Quizzes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Quizer.Models.Quizzes;

namespace Quizer.Controllers.Tests
{
    [TestClass()]
    public class QuestionControllerTests
    {
        [TestMethod()]
        public async Task EditTestAsync()
        {
            QuestionController questionController = new QuestionController(GetScopeFactoryMock(), GetUserManagerMock("0"));
            await questionController.Edit("0", "0", "{\"Position\" : 1, \"Title\" : \"title_2\", \"Image\" : \"no_image\", \"Answers\" : [{\"Title\" : \"Yep\", \"IsCorrect\" : true}]}}");
        }

        private IServiceScopeFactory GetScopeFactoryMock()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.
                Setup(x => x.GetService(typeof(IQuestionDataRepository))).
                Returns(GetQuestionDataRepositoryMock());
            serviceProvider
                .Setup(x => x.GetService(typeof(UserManager<ApplicationUser>)))
                .Returns(GetUserManagerMock("0"));
            serviceProvider
                .Setup(x => x.GetService(typeof(IParticipatorRepository)))
                .Returns(new Mock<IParticipatorRepository>().Object);

            var serviceScope = new Mock<IServiceScope>();
            serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            serviceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(serviceScope.Object);

            serviceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactory.Object);

            return serviceScopeFactory.Object;
        }

        private IQuestionDataRepository GetQuestionDataRepositoryMock()
        {
            var mock = new Mock<IQuestionDataRepository>();
            mock.Setup(x => x.GetUserQuizQuestionData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).
                Returns(new QuestionData("0", 
                new QuestionInfo(0, "title", QuestionType.Test),
                new List<AnswerData>()));
            mock.Setup(x => x.UpdateUserQuizQuestion(
                It.Is<string>(x => x == "0"),
                It.Is<string>(x => x == "0"),
                It.Is<string>(x => x == "0"),
                It.Is<QuestionInfo>(x => x.Title == "title_2" && x.Type == QuestionType.TextEntry && x.Position == 1),
                It.Is<List<AnswerInfo>>(x => x.First().Title == "Yep" && x.First().IsCorrect == true)
                ));

            return mock.Object;
        }

        private UserManager<ApplicationUser> GetUserManagerMock(string userId)
        {
            List<ApplicationUser> users = new List<ApplicationUser>()
            {
                new ApplicationUser()
                {
                    Id = userId,
                    UserName = "test",
                    DisplayName = "Test"
                },
            };
            UserManager<ApplicationUser> userManager = MockUserManager<ApplicationUser>(users).Object;

            return userManager;
        }

        private Mock<UserManager<TUser>> MockUserManager<TUser>(List<TUser> ls) where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<TUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

            mgr.Setup(x => x.DeleteAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<TUser, string>((x, y) => ls.Add(x));
            mgr.Setup(x => x.UpdateAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(ls.First());
            mgr.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(ls.First());

            return mgr;
        }
    }
}