using FluentResults;

namespace Quizer.Exceptions.Services
{
    public class QrNotFoundError : Error
    {
        public QrNotFoundError()
        {
        }

        public QrNotFoundError(string message)
            : base(message)
        {
        }
    }
}
