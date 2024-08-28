using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Lueben.Microservice.GenericEmail.Models.Requests;
using Lueben.Microservice.GenericEmail.Models.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PactNet.Mocks.MockHttpService.Models;

namespace Lueben.Microservice.GenericEmail.Tests.Pact
{
    public static class TestFactory
    {
        public static GenericEmailServiceClient CreateGenericEmailServiceClient(GenericEmailServiceOptions settings, LoggerFactory loggerFactory)
        {
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(new HttpClientHandler()));

            var restClientFactory = new EmailFactory(loggerFactory, httpClientFactoryMock.Object);
            var settingsOptions = Options.Create(settings);
            var settingsOptionsSnapshot = new Mock<IOptionsSnapshot<GenericEmailServiceOptions>>();
            settingsOptionsSnapshot.Setup(x => x.Value)
                .Returns(settingsOptions.Value);

            var service = new GenericEmailServiceClient(restClientFactory, settingsOptionsSnapshot.Object);
            return service;
        }

        public static ProviderServiceResponse CreateApiResponse(string statusCode, string statusCodeMessage, HttpStatusCode httpStatusCode, string[] errorMessages = null)
        {
            return new ProviderServiceResponse
            {
                Status = (int)httpStatusCode,
                Headers = new Dictionary<string, object>
                {
                    {"Content-Type", "application/json; charset=utf-8"}
                },
                Body = new ApiResponse
                {
                    Status = statusCode,
                    Message = statusCodeMessage,
                    Errors = errorMessages == null ? null : new List<ApiErrorResponse>
                    {
                        new()
                        {
                            Code = (int)httpStatusCode,
                            Messages = errorMessages
                        }
                    }
                }
            };
        }

        public static ProviderServiceRequest CreateAuthorizedPostRequest(GenericEmailRequest requestBody)
        {
            return new ProviderServiceRequest
            {
                Method = HttpVerb.Post,
                Path = GenericEmailServiceTests.Prefix + GenericEmailServiceClient.GenericEmailServiceApiUrl,
                Headers = new Dictionary<string, object>
                {
                    {"Content-Type", "application/json; charset=utf-8"}},
                Body = requestBody
            };
        }

        public static ProviderServiceRequest CreateAuthorizedPostDynamicEmailRequest(EmailRequest requestBody)
        {
            return new ProviderServiceRequest
            {
                Method = HttpVerb.Post,
                Path = GenericEmailServiceTests.Prefix + GenericEmailServiceClient.GenericDynamicEmailServiceApiUrl,
                Headers = new Dictionary<string, object>
                {
                    {"Content-Type", "application/json; charset=utf-8"},
                },
                Body = requestBody
            };
        }

        public static ProviderServiceRequest CreateAuthorizedPostSendOnBehalfOfServiceEmailRequest(EmailRequest requestBody)
        {
            return new ProviderServiceRequest
            {
                Method = HttpVerb.Post,
                Path = GenericEmailServiceTests.Prefix + GenericEmailServiceClient.GenericOnBehalfEmailServiceApiUrl,
                Headers = new Dictionary<string, object>
                {
                    {"Content-Type", "application/json; charset=utf-8"}
                },
                Body = requestBody
            };
        }

        public static ProviderServiceResponse CreateApiErrorResponse(HttpStatusCode httpStatusCode, string[] errorMessages) => CreateApiResponse(GenericEmailServiceTests.FailedStatus, GenericEmailServiceTests.FailedStatusMessage, httpStatusCode, errorMessages);

        public static ProviderServiceResponse CreateApiOkResponse() => CreateApiResponse(GenericEmailServiceTests.SuccessStatus, GenericEmailServiceTests.SuccessStatusMessage, HttpStatusCode.OK);
    }
}