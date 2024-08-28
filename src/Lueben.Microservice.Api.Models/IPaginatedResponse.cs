using System.Collections.Generic;

namespace Lueben.Microservice.Api.Models
{
    public interface IPaginatedResponse<T>
    {
        IList<T> Items { get; set; }

        int TotalPages { get; set; }

        int TotalItems { get; set; }
    }
}