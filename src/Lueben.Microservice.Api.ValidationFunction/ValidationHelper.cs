using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Lueben.Microservice.Api.Models;
using Lueben.Microservice.Api.ValidationFunction.Exceptions;
using Lueben.Microservice.Api.ValidationFunction.Models;
using Newtonsoft.Json;

namespace Lueben.Microservice.Api.ValidationFunction
{
    public class ValidationHelper
    {
        public static async Task<ValidatedResult<T>> GetValidatedResult<T>(IValidator<T> validator, T requestObject, string location = "body")
        {
            var validationResult = validator != null ? await validator.ValidateAsync(requestObject) : new ValidationResult();

            var validatedRequest = new ValidatedResult<T>
            {
                Value = requestObject,
                IsValid = validationResult.IsValid
            };

            if (!validationResult.IsValid)
            {
                validatedRequest.Errors = validationResult.Errors.Select(e => new ValidationError
                {
                    Field = (string)e.FormattedMessagePlaceholderValues.First(x => x.Key == "PropertyName").Value,
                    Issue = e.ErrorMessage,
                    Value = e.AttemptedValue?.ToString(),
                    Location = location
                }).ToList();
            }

            return validatedRequest;
        }

        public static T DeserializeObject<T>(string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (JsonSerializationException ex)
            {
                throw new JsonBodyNotValidException(ex.Path, ex.Message);
            }
            catch (JsonReaderException ex)
            {
                throw new JsonBodyNotValidException(ex.Path, ex.Message);
            }
        }
    }
}
