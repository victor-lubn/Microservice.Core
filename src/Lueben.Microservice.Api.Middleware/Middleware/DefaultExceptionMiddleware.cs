using System;
using System.Net;
using Lueben.Microservice.Api.Middleware.Constants;
using Lueben.Microservice.Api.Models;

namespace Lueben.Microservice.Api.Middleware.Middleware
{
    public class DefaultExceptionMiddleware : ExceptionHandlingMiddleware
    {
        public override ErrorResult GetErrorResult(Exception exception)
        {
            var errorMessage = exception.ToString();

            return new ErrorResult(errorMessage, HttpStatusCode.InternalServerError, ErrorNames.InternalServerError);
        }
    }
}
