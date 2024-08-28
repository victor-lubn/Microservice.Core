using System.Collections.Generic;
using Lueben.Microservice.Api.Models;

namespace Lueben.Microservice.Api.ValidationFunction.Models
{
    public class ValidatedResult<T>
    {
        public T Value { get; set; }

        public bool IsValid { get; set; }

        public IList<ValidationError> Errors { get; set; }
    }
}