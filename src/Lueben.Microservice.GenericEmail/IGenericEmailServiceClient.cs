using System.Threading.Tasks;
using Lueben.Microservice.GenericEmail.Models.Requests;
using Lueben.Microservice.GenericEmail.Models.Responses;

namespace Lueben.Microservice.GenericEmail
{
    public interface IGenericEmailServiceClient
    {
       Task<ApiResponse> SendEmail(GenericEmailRequest genericEmailRequest);

       Task<ApiResponse> SendDynamicEmail(DynamicEmailRequest dynamicEmailRequest);

       Task SendOnBehalfOfServiceEmail(GenericEmailRequest genericEmailRequest);

       Task<string> GetVersion();
    }
}