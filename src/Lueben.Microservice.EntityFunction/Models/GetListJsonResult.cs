using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Lueben.Microservice.Api.Models;
using Newtonsoft.Json;

namespace Lueben.Microservice.EntityFunction.Models
{
    [ExcludeFromCodeCoverage]
    public class GetListJsonResult<T> : GetJsonResultBase
    {
        public IList<T> Items { get; }

        public int TotalPages { get; }

        public int TotalItems { get; }

        public GetListJsonResult(PaginatedResponse<T> paginatedResponse)
            : base(new { paginatedResponse.Items, paginatedResponse.TotalItems, paginatedResponse.TotalPages })
        {
            Items = paginatedResponse.Items;
            TotalItems = paginatedResponse.TotalItems;
            TotalPages = paginatedResponse.TotalPages;
            StatusCode = (int)HttpStatusCode.OK;
        }

        [ExcludeFromCodeCoverage]
        protected override string SerializeObject()
        {
            return JsonConvert.SerializeObject(new { Items, TotalItems, TotalPages });
        }
    }
}
