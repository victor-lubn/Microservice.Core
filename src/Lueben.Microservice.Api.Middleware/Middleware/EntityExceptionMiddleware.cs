using System;
using System.Net;
using Lueben.Microservice.Api.Middleware.Constants;
using Lueben.Microservice.Api.Middleware.Exceptions;
using Lueben.Microservice.Api.Models;

namespace Lueben.Microservice.Api.Middleware.Middleware
{
    public class EntityExceptionMiddleware : ExceptionHandlingMiddleware
    {
        public override ErrorResult GetErrorResult(Exception exception)
        {
            return exception switch
            {
                EntityNotFoundException _ => new ErrorResult(exception.Message, HttpStatusCode.NotFound, ErrorNames.EntityNotFound),
                _ => null
            };
        }
    }
}
