using FluentResults;

namespace Quizer.Exceptions.Services
{
    public class QuestionAlreadyAnsweredError : Error
    {
        public QuestionAlreadyAnsweredError()
        {
        }

        public QuestionAlreadyAnsweredError(string message)
            : base(message)
        {
        }
    }
}
