using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Lueben.Microservice.Api.ValidationFunction.Constants;
using Lueben.Microservice.Api.ValidationFunction.Exceptions;
using Lueben.Microservice.Api.ValidationFunction.Models;
using Microsoft.Azure.Functions.Worker.Http;

namespace Lueben.Microservice.Api.ValidationFunction.Extensions
{
    public static class HttpRequestExtensions
    {
        public static async Task<ValidatedResult<T>> GetRequestValidatedResult<T>(this HttpRequestData request, IValidator<T> validator, bool allowEmptyModel = false)
        {
            var requestBody = await request.ReadAsStringAsync();
            var requestObject = ValidationHelper.DeserializeObject<T>(requestBody);

            if (requestObject == null)
            {
                throw new JsonBodyNotValidException("body", ErrorMessages.EmptyBodyError);
            }

            if (!allowEmptyModel && !requestObject.GetFilledProperties().Any())
            {
                throw new ModelDoesNotHaveAnyPropertiesException();
            }

            return await ValidationHelper.GetValidatedResult(validator, requestObject);
        }
    }
}