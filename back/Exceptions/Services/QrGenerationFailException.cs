namespace Quizer.Exceptions.Services
{
    public class QrGenerationFailException : Exception
    {
        public QrGenerationFailException()
        {
        }

        public QrGenerationFailException(string message)
            : base(message)
        {
        }

        public QrGenerationFailException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
