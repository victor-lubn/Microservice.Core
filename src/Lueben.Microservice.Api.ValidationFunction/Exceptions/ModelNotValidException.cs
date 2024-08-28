using System;
using System.Collections.Generic;
using Lueben.Microservice.Api.Models;
using Lueben.Microservice.Api.ValidationFunction.Constants;

namespace Lueben.Microservice.Api.ValidationFunction.Exceptions
{
    public class ModelNotValidException : Exception
    {
        public IList<ValidationError> ValidationErrors { get; set; }

        public ModelNotValidException(IList<ValidationError> validationErrors) : base(ErrorMessages.ModelNotValidError)
        {
            ValidationErrors = validationErrors;
        }

        public ModelNotValidException(string field, string errorMessage, string location = "body") : base(ErrorMessages.ModelNotValidError)
        {
            ValidationErrors = new List<ValidationError>
            {
                new()
                {
                    Field = field,
                    Issue = errorMessage,
                    Location = location
                }
            };
        }
    }
}