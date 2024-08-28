namespace Lueben.Microservice.Notification.Models
{
    public class DepotChannel
    {
        public string Message { get; set; }

        public string ContactType { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string CustomerEmail { get; set; }

        public string CustomerPhone { get; set; }

        public string UserType { get; set; }

        public string EnquiryType { get; set; }

        public object Data { get; set; }
    }
}
