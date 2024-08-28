using System.Collections.Generic;
using Lueben.Microservice.Api.Models;

namespace Lueben.Microservice.OpenApi.Attributes
{
    public class OpenApiPaginatedResponse<T> : IPaginatedResponse<T>
    {
        public IList<T> Items { get; set; }

        public int TotalPages { get; set; }

        public int TotalItems { get; set; }
    }
}