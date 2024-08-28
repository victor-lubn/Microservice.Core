using System.Net;

namespace Lueben.Microservice.Api.Models
{
    public class ErrorResultConflict : ErrorResult
    {
        public ErrorResultConflict(string message, string name, object entity)
            : base(message, HttpStatusCode.Conflict, name)
        {
            Object = entity;
        }

        public object Object { get; set; }
    }
}
