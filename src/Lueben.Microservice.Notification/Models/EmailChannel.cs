namespace Lueben.Microservice.Notification.Models
{
    public class EmailChannel
    {
        public object From { get; set; }

        public object Cc { get; set; }

        public object Bcc { get; set; }

        public object To { get; set; }

        public object Subject { get; set; }

        public object Body { get; set; }
    }
}
