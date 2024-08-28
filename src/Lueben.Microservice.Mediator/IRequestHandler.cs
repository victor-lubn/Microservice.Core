using System.Threading.Tasks;

namespace Lueben.Microservice.Mediator
{
    public interface IRequestHandler<TRequest, TResponse>
    {
        Task<TResponse> Handle(TRequest query);
    }
}
