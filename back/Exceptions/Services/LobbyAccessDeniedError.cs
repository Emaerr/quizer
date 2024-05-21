using FluentResults;

namespace Quizer.Exceptions.Services
{
    public class LobbyAccessDeniedError : Error
    {
        public LobbyAccessDeniedError()
        {
        }

        public LobbyAccessDeniedError(string message)
            : base(message)
        {
        }
    }
}
