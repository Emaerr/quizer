using FluentResults;

namespace Quizer.Exceptions.Services
{
    public class UserAlreadyInLobbyError : Error
    {
        public UserAlreadyInLobbyError()
        {
        }

        public UserAlreadyInLobbyError(string message)
            : base(message)
        {
        }
    }
}
