using FluentResults;

namespace Quizer.Exceptions.Services
{
    public class InvalidAnswerGuidError : Error
    {
        public InvalidAnswerGuidError()
        {
        }

        public InvalidAnswerGuidError(string message)
            : base(message)
        {
        }
    }
}
