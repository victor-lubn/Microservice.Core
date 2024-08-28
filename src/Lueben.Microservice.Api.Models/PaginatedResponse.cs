using System.Collections.Generic;
using System.Net;

namespace Lueben.Microservice.Api.Models
{
    public class PaginatedResponse<T> : ErrorResult, IPaginatedResponse<T>
    {
        public PaginatedResponse() : base(null, HttpStatusCode.OK, null)
        {
        }

        public PaginatedResponse(IList<T> items, int totalPages, int totalItems) : base(null, HttpStatusCode.OK, null)
        {
            Items = items;
            TotalPages = totalPages;
            TotalItems = totalItems;
        }

        public PaginatedResponse(string message, HttpStatusCode statusCode, string name, IList<ValidationError> details = null) : base(message, statusCode, name, details)
        {
        }

        public IList<T> Items { get; set; }

        public int TotalPages { get; set; }

        public int TotalItems { get; set; }
    }
}
