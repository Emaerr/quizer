using Quizer.Data;

namespace Quizer.Services.Quizzes
{
    public class QuizService : IQuizService
    {
        private QuizRepository _quizzes;

        public QuizService(QuizContext context)
        {
            _quizzes = new QuizRepository(context);
        }


    }
}
