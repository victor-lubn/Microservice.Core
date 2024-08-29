using System;
using System.Runtime.Serialization;

namespace Lueben.Microservice.DurableFunction.Exceptions
{
    [Serializable]
    public class IncorrectEventDataException : Exception
    {
        public IncorrectEventDataException()
        {
        }

        public IncorrectEventDataException(string message) : base(message)
        {
        }

        public IncorrectEventDataException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}