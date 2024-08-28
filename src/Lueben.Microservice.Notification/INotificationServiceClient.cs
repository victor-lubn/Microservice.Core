using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lueben.Microservice.Notification.Models;

namespace Lueben.Microservice.Notification
{
    public interface INotificationServiceClient
    {
        Task<CreateNotificationResponse> SendNotificationAsync(NotificationRequest notificationRequest);

        Task<NotificationResponse> GetNotificationByIdAsync(Guid notificationId);

        Task<NotificationChannelStatusResponse> GetChannelStatusAsync(NotificationChannelStatusRequest channelStatusRequest);

        Task<IReadOnlyCollection<NotificationChannelStatusResponse>> GetChannelStatusesByIdAsync(Guid notificationId);
    }
}