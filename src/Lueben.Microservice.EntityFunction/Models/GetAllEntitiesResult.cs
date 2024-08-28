using System;
using System.Linq;

namespace Lueben.Microservice.EntityFunction.Models
{
    public class GetAllEntitiesResult<T>
    {
        public GetAllEntitiesResult(IQueryable<T> items, int totalItems, int pageSize)
        {
            Items = items;
            TotalItems = totalItems;
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        }

        public IQueryable<T> Items { get; set; }

        public int TotalItems { get; set; }

        public int TotalPages { get; set; }
    }
}
