using System.Collections.Generic;
using System.Net;

namespace Lueben.Microservice.Api.Models
{
    public class Response<T> : ErrorResult
    {
        public Response() : base(null, HttpStatusCode.OK, null)
        {
        }

        public Response(T result) : base(null, HttpStatusCode.OK, null)
        {
            Result = result;
        }

        public Response(string message, HttpStatusCode statusCode, string name, IList<ValidationError> details = null) : base(message, statusCode, name, details)
        {
        }

        public T Result;
    }
}