//using Microsoft.EntityFrameworkCore;
//using Quizer.Models.Quizzes;
//using Quizer.Models.User;

//namespace Quizer.Data
//{
//    public class AppDbContextSeed
//    {
//        private static List<Question> _questions = new List<Question>() {
//            new Question() {Title = "test", Position = 0, Type = QuestionType.Test, Answers = new List<Answer>() {

//                new Answer() { }
//            } 
//            },
//        };

//        public static async Task SeedAsync(AppDbContext context,
//        ILogger logger,
//        int retry = 0)
//        {
//            var retryForAvailability = retry;
//            try
//            {
//                //if (context.Database.IsSqlServer())
//                //{
//                //    context.Database.Migrate();
//                //}

//                if (!await context.Quizzes.AnyAsync())
//                {
//                    await context.Quizzes.AddRangeAsync(
//                            GetPreconfiguredQuizzes());

//                    await context.SaveChangesAsync();
//                }

//                if (!await context.Questions.AnyAsync())
//                {
//                    await context.Questions.AddRangeAsync(
//                            GetPreconfiguredQuestions());

//                    await context.SaveChangesAsync();
//                }

//                if (!await context.Users.AnyAsync())
//                {
//                    await context.Users.AddRangeAsync(
//                        GetPreconfiguredUsers());

//                    await context.SaveChangesAsync();
//                }
//            }
//            catch (Exception ex)
//            {
//                if (retryForAvailability >= 10) throw;

//                retryForAvailability++;

//                logger.LogError(ex.Message);
//                await SeedAsync(context, logger, retryForAvailability);
//                throw;
//            }
//        }

//        static IEnumerable<Quiz> GetPreconfiguredQuizzes()
//        {
//            return new List<Quiz>
//            {
//                new() {AuthorId = "0", BreakTime = 5, TimeLimit = 10, },
//            };
//        }

//        static IEnumerable<ApplicationUser> GetPreconfiguredUsers()
//        {
//            return new List<ApplicationUser>
//            {
//                new ApplicationUser()
//                {
//                    Id = "0",
//                },
//            };
//        }

//        static IEnumerable<Question> GetPreconfiguredQuestions()
//        {
//            return _questions;
//        }
//    }
//}
