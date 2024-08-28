namespace Lueben.Microservice.Notification.Models
{
    public class NotificationChannel
    {
        public EmailChannel Email { get; set; }

        public PushChannel Push { get; set; }

        public InAppChannel InApp { get; set; }

        public DepotChannel Depot { get; set; }

        public SmsChannel Sms { get; set; }
    }
}
