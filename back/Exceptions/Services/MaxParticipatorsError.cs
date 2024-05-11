using FluentResults;
using Quizer.Exceptions.Models;

namespace Quizer.Exceptions.Services
{
    public class MaxParticipatorsError : Error
    {
        public MaxParticipatorsError()
        {
        }

        public MaxParticipatorsError(string message)
            : base(message)
        {
        }

    }
}
