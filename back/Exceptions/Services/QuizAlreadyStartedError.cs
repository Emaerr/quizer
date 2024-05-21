using FluentResults;

namespace Quizer.Exceptions.Services
{
    public class QuizAlreadyStartedError : Error
    {
        public QuizAlreadyStartedError()
        {
        }

        public QuizAlreadyStartedError(string message)
            : base(message)
        {
        }
    }
}
