using System.Threading.Tasks;

namespace Lueben.Microservice.CircuitBreaker.CleanUp
{
    public interface IEntityAzureStorageCleaner
    {
        Task CleanEntityHistory(EntityCleanUpOptions options);
    }
}