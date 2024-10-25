using FluentResults;

namespace Quizer.Exceptions.Services
{
    public class LobbyNotFoundError : Error
    {
        public LobbyNotFoundError()
        {
        }

        public LobbyNotFoundError(string message)
            : base(message)
        {
        }
    }
}
