using System;
using System.Diagnostics.CodeAnalysis;
using Lueben.Microservice.Json.Encrypt.Constants;

namespace Lueben.Microservice.Json.Encrypt.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class EncryptionTokenInvalidException : Exception
    {
        public EncryptionTokenInvalidException()
            : base(ErrorMessages.CryptographicOperationError)
        {
        }

        public EncryptionTokenInvalidException(string message)
            : base(message)
        {
        }

        public EncryptionTokenInvalidException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
