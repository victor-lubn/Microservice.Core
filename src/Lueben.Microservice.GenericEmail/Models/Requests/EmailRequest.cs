namespace Lueben.Microservice.GenericEmail.Models.Requests
{
    public class EmailRequest
    {
        public string To { get; set; }

        public string Source { get; set; }

        public string Subject { get; set; }

        public string EmailType { get; set; }
    }
}
