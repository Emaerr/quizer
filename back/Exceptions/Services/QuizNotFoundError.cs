using FluentResults;

namespace Quizer.Exceptions.Services
{
    public class QuizNotFoundError : Error
    {
        public QuizNotFoundError()
        {
        }

        public QuizNotFoundError(string message)
            : base(message)
        {
        }
    }
}
