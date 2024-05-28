using FluentResults;

namespace Quizer.Exceptions.Services
{
    public class NotRightTimeToAnswerError : Error
    {
        public NotRightTimeToAnswerError()
        {
        }

        public NotRightTimeToAnswerError(string message)
            : base(message)
        {
        }
    }
}
