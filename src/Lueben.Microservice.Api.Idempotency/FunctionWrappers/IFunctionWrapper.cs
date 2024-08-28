using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Lueben.Microservice.Api.Idempotency.FunctionWrappers
{
    public interface IFunctionWrapper
    {
        Task<IActionResult> Execute(
            Func<Task<IActionResult>> azureFunction,
            string functionName,
            HttpStatusCode idempotencyResultStatusCode = HttpStatusCode.Created,
            bool isIdempotencyOptional = false);
    }
}
