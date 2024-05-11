using FluentResults;

namespace Quizer.Exceptions.Services
{
    public class UserNotFoundError : Error
    {
        public UserNotFoundError()
        {
        }

        public UserNotFoundError(string message)
            : base(message)
        {
        }
    }
}
