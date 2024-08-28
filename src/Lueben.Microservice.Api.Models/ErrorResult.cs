using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace Lueben.Microservice.Api.Models
{
    public class ErrorResult
    {
        public ErrorResult(string message, HttpStatusCode statusCode, string name, IList<ValidationError> details = null)
        {
            StatusCode = statusCode;
            Message = message;
            Name = name;
            Details = details;
        }

        [JsonIgnore]
        public HttpStatusCode StatusCode { get; }

        public string Name { get; }

        public string Message { get; }

        public IList<ValidationError> Details { get; }

        public string DebugId { get; set; }
    }
}