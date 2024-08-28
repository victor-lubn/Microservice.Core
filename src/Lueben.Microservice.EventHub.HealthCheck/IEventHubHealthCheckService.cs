using System.Threading.Tasks;

namespace Lueben.Microservice.EventHub.HealthCheck
{
    public interface IEventHubHealthCheckService
    {
        Task<bool> IsAvailable(string eventHubNamespace, string eventHubName);
    }
}
