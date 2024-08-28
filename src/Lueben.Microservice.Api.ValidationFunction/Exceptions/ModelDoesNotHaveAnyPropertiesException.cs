using System;
using Lueben.Microservice.Api.ValidationFunction.Constants;

namespace Lueben.Microservice.Api.ValidationFunction.Exceptions
{
    public class ModelDoesNotHaveAnyPropertiesException : Exception
    {
        public ModelDoesNotHaveAnyPropertiesException() : base(message: ErrorMessages.ModelDoesNotHaveAnyPropertiesError)
        {
        }
    }
}