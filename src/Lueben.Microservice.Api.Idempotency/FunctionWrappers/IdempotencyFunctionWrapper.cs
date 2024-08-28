using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Lueben.Microservice.Api.Idempotency.Constants;
using Lueben.Microservice.Api.Idempotency.Exceptions;
using Lueben.Microservice.Api.Idempotency.Extensions;
using Lueben.Microservice.Api.Idempotency.IdempotencyDataProviders;
using Lueben.Microservice.Api.Idempotency.Models;
using Lueben.Microservice.Api.ValidationFunction.Exceptions;
using Lueben.Microservice.Diagnostics;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lueben.Microservice.Api.Idempotency.FunctionWrappers
{
    public class IdempotencyFunctionWrapper : IFunctionWrapper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdempotencyDataProvider<IdempotencyEntity> _idempotencyDataProvider;
        private readonly ILogger<IdempotencyFunctionWrapper> _logger;

        public IdempotencyFunctionWrapper(
            IHttpContextAccessor httpContextAccessor,
            IIdempotencyDataProvider<IdempotencyEntity> idempotencyDataProvider,
            ILogger<IdempotencyFunctionWrapper> logger)
        {
            Ensure.ArgumentNotNull(idempotencyDataProvider, nameof(idempotencyDataProvider));
            Ensure.ArgumentNotNull(httpContextAccessor, nameof(httpContextAccessor));
            Ensure.ArgumentNotNull(logger, nameof(logger));

            _idempotencyDataProvider = idempotencyDataProvider;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Execute(
            Func<Task<IActionResult>> azureFunction,
            string functionName,
            HttpStatusCode idempotencyResultStatusCode = HttpStatusCode.Created,
            bool isIdempotencyOptional = false)
        {
            var idempotencyKey = GetIdempotencyKey();
            if (string.IsNullOrEmpty(idempotencyKey) && isIdempotencyOptional)
            {
                return await azureFunction();
            }

            if (string.IsNullOrEmpty(idempotencyKey))
            {
                throw new IdempotencyKeyNullOrEmptyException();
            }

            var idempotencyEntity = await this.TryGetIdempotencyEntity(idempotencyKey, functionName);
            if (idempotencyEntity == null)
            {
                return await TryExecuteFunction(azureFunction, idempotencyKey);
            }

            var response = idempotencyEntity.GetDataProviderResponse(idempotencyResultStatusCode);
            _logger.LogInformation($"Duplicate request was just returned for {idempotencyKey}.");

            return response;
        }

        private string GetIdempotencyKey()
        {
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(
                Headers.Idempotency,
                out var idempotencyHeaderValue))
            {
                return idempotencyHeaderValue.ToString();
            }

            return null;
        }

        private async Task<IdempotencyEntity> TryGetIdempotencyEntity(string idempotencyKey, string functionName)
        {
            var isValid = Regex.IsMatch(idempotencyKey, Formats.UuidRegexFormat, RegexOptions.IgnoreCase);
            if (!isValid)
            {
                throw new ModelNotValidException(Headers.Idempotency, ErrorMessages.IdempotencyKeyNotValidError, "header");
            }

            var request = _httpContextAccessor.HttpContext.Request;
            var requestBody = await request.ReadAsStringAsync();
            if (requestBody.Length == 0)
            {
                throw new JsonBodyNotValidException("body", ErrorMessages.EmptyBodyError);
            }

            var hash = HashGenerator.CreateShaHash(Encoding.ASCII.GetBytes(requestBody));
            var result = await _idempotencyDataProvider.Get(idempotencyKey);
            if (result != null)
            {
                return result.GetVerifiedEntity(hash);
            }

            await _idempotencyDataProvider.Add(idempotencyKey, hash, functionName);

            return null;
        }

        private async Task<IActionResult> TryExecuteFunction(Func<Task<IActionResult>> azureFunction, string idempotencyKey)
        {
            IActionResult functionResult;
            string responseHash;
            string entityType;
            var idempotencyEntity = await _idempotencyDataProvider.Get(idempotencyKey);
            try
            {
                functionResult = await azureFunction();

                var objectResult = (ObjectResult)functionResult;
                var jsonObject = JsonConvert.SerializeObject(objectResult.Value);
                entityType = objectResult.Value != null ?
                    objectResult.Value.GetType().FullName :
                    objectResult.GetType().FullName;

                responseHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonObject));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The exception occurred while executing of the function.");
                await _idempotencyDataProvider.Delete(idempotencyEntity);
                throw;
            }

            idempotencyEntity.Response = responseHash;
            idempotencyEntity.EntityType = entityType;
            await _idempotencyDataProvider.Update(idempotencyEntity);

            return functionResult;
        }
    }
}
