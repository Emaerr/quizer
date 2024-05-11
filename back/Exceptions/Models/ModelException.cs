namespace Quizer.Exceptions.Models
{
    public class ModelException : Exception
    {
        public ModelException()
        {
        }

        public ModelException(string message)
            : base(message)
        {
        }

        public ModelException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
