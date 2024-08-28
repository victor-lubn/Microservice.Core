using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Lueben.Microservice.EntityFunction.Models
{
    public class CreatedEmptyResult : ObjectResult
    {
        public CreatedEmptyResult()
            : base(null)
        {
            StatusCode = (int)HttpStatusCode.Created;
        }
    }
}
