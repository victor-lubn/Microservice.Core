using System.Threading.Tasks;
using Lueben.Microservice.Api.Idempotency.Models;

namespace Lueben.Microservice.Api.Idempotency.IdempotencyDataProviders
{
    public interface IIdempotencyDataProvider<T>
        where T : Entity
    {
        Task Add(string idempotencyKey, string payloadHash, string functionName);

        Task CleanUp();

        Task Delete(T entity);

        Task<T> Get(string idempotencyKey);

        Task Update(T idempotency);
    }
}
