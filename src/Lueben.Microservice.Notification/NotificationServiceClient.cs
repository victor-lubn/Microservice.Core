using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lueben.Microservice.Diagnostics;
using Lueben.Microservice.Notification.Models;
using Lueben.Microservice.Notification.Options;
using Lueben.Microservice.RestSharpClient.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.Notification
{
    public class NotificationServiceClient : ApiClient, INotificationServiceClient
    {
        private const string SendNotificationEndpoint = "notificationRequest";
        private const string GetNotificationEndpoint = "notificationRequest/{notificationId}";
        private const string GetNotificationChannelStatusEndpoint = "notificationRequest/{notificationId}/status/{channelType}";
        private const string GetNotificationStatusesEndpoint = "notificationRequest/{notificationId}/status";
        private readonly NotificationClientOptions _notificationClientOptions;
        private readonly ILogger<NotificationServiceClient> _logger;

        public NotificationServiceClient(
            IRestSharpClientFactory restSharpClientFactory,
            IOptionsSnapshot<NotificationClientOptions> notificationClientOptions,
            ILogger<NotificationServiceClient> logger) : base(restSharpClientFactory, notificationClientOptions, logger)
        {
            _logger = logger;
            _notificationClientOptions = notificationClientOptions.Value;
        }

        public async Task<CreateNotificationResponse> SendNotificationAsync(NotificationRequest notificationRequest)
        {
            Ensure.ArgumentNotNull(notificationRequest, nameof(notificationRequest));

            var methodUrl = $"{_notificationClientOptions.BaseUrl}/{NotificationServiceClient.SendNotificationEndpoint}";

            var response = await Post<CreateNotificationResponse>(methodUrl, notificationRequest);
            _logger.LogInformation($"Notification is sent. Type={notificationRequest.Type}, notificationId={response?.Id}");
            return response;
        }

        public async Task<NotificationResponse> GetNotificationByIdAsync(Guid notificationId)
        {
            var methodUrl = $"{_notificationClientOptions.BaseUrl}/{NotificationServiceClient.GetNotificationEndpoint}";

            return await Get<NotificationResponse>(methodUrl, null, null, notificationId);
        }

        public async Task<NotificationChannelStatusResponse> GetChannelStatusAsync(NotificationChannelStatusRequest channelStatusRequest)
        {
            Ensure.ArgumentNotNull(channelStatusRequest, nameof(channelStatusRequest));

            var methodUrl = $"{_notificationClientOptions.BaseUrl}/{NotificationServiceClient.GetNotificationChannelStatusEndpoint}";

            return await Get<NotificationChannelStatusResponse>(methodUrl, null, null, channelStatusRequest.NotificationId, channelStatusRequest.ChannelType);
        }

        public async Task<IReadOnlyCollection<NotificationChannelStatusResponse>> GetChannelStatusesByIdAsync(Guid notificationId)
        {
            var methodUrl = $"{_notificationClientOptions.BaseUrl}/{NotificationServiceClient.GetNotificationStatusesEndpoint}";

            return await Get<List<NotificationChannelStatusResponse>>(methodUrl, null, null, notificationId);
        }
    }
}
