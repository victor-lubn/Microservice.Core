namespace Lueben.Microservice.Notification.Models
{
    public class NotificationRequest
    {
        public string LocaleCode { get; set; }

        public string Type { get; set; }

        public string Source { get; set; }

        public NotificationChannel Channel { get; set; }

        public Recipient Recipient { get; set; }
    }
}
