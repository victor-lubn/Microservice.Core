namespace Lueben.Microservice.GenericEmail.Models.Requests
{
    public class DynamicEmailRequest : EmailRequest
    {
        public object Parameters { get; set; }
    }
}
