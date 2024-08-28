using System;
using System.Net;
using Lueben.Microservice.Api.Middleware.Middleware;
using Lueben.Microservice.Api.Models;
using Lueben.Microservice.Localization.Constants;
using Lueben.Microservice.Localization.Exceptions;

namespace Lueben.Microservice.Localization.Middleware
{
    public class LocaleCodeNotSupportedMiddleware : ExceptionHandlingMiddleware
    {
        public override ErrorResult GetErrorResult(Exception exception)
        {
            switch (exception)
            {
                case LocaleCodeNotSupportedException _:
                {
                    return new ErrorResult(exception.Message, HttpStatusCode.NotAcceptable, ErrorNames.NotAcceptable);
                }

                case LocaleCodeHeaderNotFoundException _:
                {
                    return new ErrorResult(exception.Message, HttpStatusCode.BadRequest, ErrorNames.BadRequest);
                }
            }

            return null;
        }
    }
}
