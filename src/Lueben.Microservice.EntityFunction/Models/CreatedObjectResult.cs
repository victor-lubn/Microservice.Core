using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Lueben.Microservice.EntityFunction.Models
{
    public class CreatedObjectResult<T> : ObjectResult
    {
        public CreatedObjectResult(T data)
            : base(new EntityResult<T> { Data = data })
        {
            Data = data;
            StatusCode = (int)HttpStatusCode.Created;
        }

        public T Data { get; }
    }
}
