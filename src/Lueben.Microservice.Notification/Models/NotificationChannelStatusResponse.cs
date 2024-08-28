using System;

namespace Lueben.Microservice.Notification.Models
{
    public class NotificationChannelStatusResponse
    {
        public string Id { get; set; }

        public string ChannelType { get; set; }

        public string Status { get; set; }

        public object Parameters { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }

        public string LocaleCode { get; set; }
    }
}
