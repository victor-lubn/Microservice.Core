using System.Diagnostics.CodeAnalysis;
using System.Net;
using Lueben.Microservice.Api.Models;
using Newtonsoft.Json;

namespace Lueben.Microservice.EntityFunction.Models
{
    [ExcludeFromCodeCoverage]
    public class GetJsonResult<T> : GetJsonResultBase
    {
        public GetJsonResult(Response<T> response)
            : base(response.Result)
        {
            StatusCode = (int)HttpStatusCode.OK;
        }

        protected override string SerializeObject()
        {
            return JsonConvert.SerializeObject(Value);
        }
    }
}