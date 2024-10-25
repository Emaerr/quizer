using FluentResults;

namespace Quizer.Exceptions.Services
{
    public class InvalidAnswerFormatError : Error
    {
        public InvalidAnswerFormatError()
        {
        }

        public InvalidAnswerFormatError(string message)
            : base(message)
        {
        }
    }
}
