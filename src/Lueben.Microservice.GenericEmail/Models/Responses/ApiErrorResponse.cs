namespace Lueben.Microservice.GenericEmail.Models.Responses
{
    public class ApiErrorResponse
    {
        public int Code { get; set; }

        public string[] Messages { get; set; }
    }
}