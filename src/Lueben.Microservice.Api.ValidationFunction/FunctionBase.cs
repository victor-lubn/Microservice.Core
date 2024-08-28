using System.Threading.Tasks;
using FluentValidation;
using Lueben.Microservice.Api.ValidationFunction.Exceptions;
using Lueben.Microservice.Api.ValidationFunction.Extensions;
using Microsoft.Azure.Functions.Worker.Http;

namespace Lueben.Microservice.Api.ValidationFunction
{
    public class FunctionBase<T>
    {
        private readonly AbstractValidator<T> _validator;

        public FunctionBase(AbstractValidator<T> validator)
        {
            _validator = validator;
        }

        public async Task<T> GetValidatedRequest(HttpRequestData request, bool allowEmptyPayload = false)
        {
            var form = await request.GetRequestValidatedResult(_validator, allowEmptyPayload);
            if (form.IsValid)
            {
                return form.Value;
            }

            throw new ModelNotValidException(form.Errors);
        }
    }
}