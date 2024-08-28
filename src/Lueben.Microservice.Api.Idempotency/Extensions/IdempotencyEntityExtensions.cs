using System;
using System.Linq;
using System.Net;
using Lueben.Microservice.Api.Idempotency.Exceptions;
using Lueben.Microservice.Api.Idempotency.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Lueben.Microservice.Api.Idempotency.Extensions
{
    public static class IdempotencyEntityExtensions
    {
        public static IActionResult GetDataProviderResponse(this IdempotencyEntity idempotencyEntity, HttpStatusCode idempotencyResultStatusCode)
        {
            var type = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a => a.GetType(idempotencyEntity.EntityType))
                .FirstOrDefault(t => t != null);

            var httpResponse = JsonConvert.DeserializeObject(
                System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(idempotencyEntity.Response)),
                type ?? throw new ArgumentNullException($"{nameof(idempotencyEntity.EntityType)} not found."));

            return new ObjectResult(httpResponse)
            {
                StatusCode = (int)idempotencyResultStatusCode,
            };
        }

        public static IdempotencyEntity GetVerifiedEntity(this IdempotencyEntity entity, string payloadHash)
        {
            if (entity == null)
            {
                return null;
            }

            if (entity.Response == null)
            {
                throw new IdempotencyConflictException();
            }

            if (entity.PayloadHash != payloadHash)
            {
                throw new IdempotencyPayloadException();
            }

            return entity;
        }
    }
}
