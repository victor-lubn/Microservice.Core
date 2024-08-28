using System;
using System.Net;
using Lueben.Microservice.Api.Middleware.Constants;
using Lueben.Microservice.Api.Middleware.Middleware;
using Lueben.Microservice.Api.Models;
using Lueben.Microservice.Api.ValidationFunction.Exceptions;

namespace Lueben.Microservice.Api.ValidationFunction.Middleware
{
    public class ModelValidationExceptionMiddleware : ExceptionHandlingMiddleware
    {
        public override ErrorResult GetErrorResult(Exception exception)
        {
            return exception switch
            {
                ModelNotValidException ex => new ErrorResult(ex.Message, HttpStatusCode.BadRequest, ErrorNames.ModelNotValid, ex.ValidationErrors),
                _ => null
            };
        }
    }
}
