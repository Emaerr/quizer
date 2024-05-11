using FluentResults;
using Quizer.Exceptions.Models;

namespace Quizer.Exceptions.Services
{
    public class LobbyUnavailableError : Error
    {
        public LobbyUnavailableError()
        {
        }

        public LobbyUnavailableError(string message)
            : base(message)
        {
        }
    }
}
