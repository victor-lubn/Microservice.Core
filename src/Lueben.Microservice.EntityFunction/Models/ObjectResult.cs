using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Lueben.Microservice.EntityFunction.Models
{
    [ExcludeFromCodeCoverage]
    public class ObjectResult<T> : ObjectResult
    {
        public ObjectResult(T data)
           : base(new EntityResult<T> { Data = data })
        {
            Data = data;
            StatusCode = (int)HttpStatusCode.OK;
        }

        public T Data { get; }
    }
}
