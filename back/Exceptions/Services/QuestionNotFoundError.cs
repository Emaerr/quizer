using FluentResults;

namespace Quizer.Exceptions.Services
{
    public class QuestionNotFoundError : Error
    {
        public QuestionNotFoundError()
        {
        }

        public QuestionNotFoundError(string message)
            : base(message)
        {
        }
    }
}
