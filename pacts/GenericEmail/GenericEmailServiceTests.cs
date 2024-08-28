using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;
using Xunit;
using Lueben.Microservice.RestSharpClient;
using Microsoft.Extensions.Logging;
using Lueben.Microservice.GenericEmail.Models.Requests;
using Lueben.Microservice.GenericEmail.Models.Responses;

namespace Lueben.Microservice.GenericEmail.Tests.Pact
{
    public class GenericEmailServiceTests : IClassFixture<EmailPactClassFixture>, IDisposable
    {
        private const string EmailDataSource = "OnlineAccountApplication";

        internal const string FailedStatus = "STATUS_FAILED";
        internal const string FailedStatusMessage = "Unable to submit your request this time";
        internal const string SuccessStatus = "STATUS_SUCCESS";
        internal const string SuccessStatusMessage = "Thank you for submitting your request";

        private readonly IMockProviderService _mockProviderService;
        private readonly GenericEmailServiceOptions _settings;
        private readonly LoggerFactory _loggerFactory;
        internal const string Prefix = "/GenericEmailService";
        private bool _disposed;

        public GenericEmailServiceTests(EmailPactClassFixture fixture)
        {
            _mockProviderService = fixture.MockProviderService;
            _mockProviderService.ClearInteractions();

            _settings = new GenericEmailServiceOptions
            {
                GenericEmailServiceApiBaseUrl = fixture.MockProviderServiceBaseUri + Prefix
            };

            _loggerFactory = new LoggerFactory();
        }

        [Fact]
        public async Task GivenSendEmailRequest_WhenNotRequiredFieldsAreEmpty_ThenTheResponseIsOk()
        {
            var request = new GenericEmailRequest
            {
                To = "address@email.com",
                Source = PactClassFixture.ServiceConsumer,
                Subject = "Subject",
                Body = "Body",
                EmailType = null
            };

            _mockProviderService.UponReceiving("Request with empty not required fields")
                .With(TestFactory.CreateAuthorizedPostRequest(request))
                .WillRespondWith(TestFactory.CreateApiOkResponse());

            var service = TestFactory.CreateGenericEmailServiceClient(_settings, _loggerFactory);
            var result = await service.SendEmail(request);

            Assert.Equal(SuccessStatusMessage, result.Message);
            Assert.Equal(SuccessStatus, result.Status);
            Assert.Null(result.Errors);
        }

        [Theory]
        [InlineData(null, "OnlineAccountApplication", "Subject", "Body", "Submitted")]
        [InlineData("address@email.com", null, "Subject", "Body", "Submitted")]
        [InlineData("address@email.com", "OnlineAccountApplication", null, "Body", "Submitted")]
        [InlineData("address@email.com", "OnlineAccountApplication", "Subject", null, "Submitted")]
        public async Task GivenSendEmailMethod_WhenOneOfRequiredFieldIsEmpty_ThenReturnBadRequestWithOneValidationError(string to, string source, string subject, string body, string emailType)
        {
            var request = new GenericEmailRequest
            {
                To = to,
                Source = source,
                Subject = subject,
                Body = body,
                EmailType = emailType
            };

            var emptyProperty = request.GetType().GetProperties().First(pi => pi.GetValue(request) == null).Name;
            var validationMessage = $"The {emptyProperty} field is required.";

            _mockProviderService.UponReceiving($"Request with 1 empty required field '{emptyProperty}'")
                .With(TestFactory.CreateAuthorizedPostRequest(request))
                .WillRespondWith(TestFactory.CreateApiErrorResponse(HttpStatusCode.BadRequest, new[] { validationMessage }));

            var service = TestFactory.CreateGenericEmailServiceClient(_settings, _loggerFactory);
            var ex = await Assert.ThrowsAsync<RestClientApiException>(() => service.SendEmail(request));
            var result = JsonConvert.DeserializeObject<ApiResponse>(ex.ResponseContent);

            Assert.Equal(FailedStatus, result.Status);
            Assert.Equal(FailedStatusMessage, result.Message);
            Assert.Single(result.Errors);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.Errors.First().Code);
            Assert.Single(result.Errors.First().Messages);
            Assert.Equal(validationMessage, result.Errors.First().Messages.First());
        }

        [Fact]
        public async Task GivenSendEmailRequest_WhenMultipleRequiredFieldAreEmpty_ThenResponseIs400WithMultipleErrorMessages()
        {
            var emptyToMessage = $"The {nameof(GenericEmailRequest.To)} field is required.";
            var emptySourceMessage = $"The {nameof(GenericEmailRequest.Source)} field is required.";

            var request = new GenericEmailRequest
            {
                Subject = "Subject",
                Body = "Body",
                EmailType = "Submitted"
            };

            _mockProviderService.UponReceiving("Request with 2 empty required fields 'To' and 'Source'")
                .With(TestFactory.CreateAuthorizedPostRequest(request))
                .WillRespondWith(TestFactory.CreateApiErrorResponse(HttpStatusCode.BadRequest, new[] { emptyToMessage, emptySourceMessage }));

            var service = TestFactory.CreateGenericEmailServiceClient(_settings, _loggerFactory);
            var ex = await Assert.ThrowsAsync<RestClientApiException>(() => service.SendEmail(request));

            Assert.NotNull(ex.ResponseData);
            var result = (ApiResponse)ex.ResponseData;
            Assert.Equal(FailedStatus, result.Status);
            Assert.Equal(FailedStatusMessage, result.Message);
            Assert.Single(result.Errors);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.Errors.First().Code);
            Assert.Equal(2, result.Errors.First().Messages.Length);
        }

        [Fact]
        public async Task GivenSendEmailRequest_WhenEmailExceedsMaxLength_ThenResponseIs400With1ErrorMessage()
        {
            const int mockEmailMaxLength = 50;

            var tooLongEmailMessage = $"The field Email exceeds maximum length of {mockEmailMaxLength} characters.";

            var request = new DynamicEmailRequest
            {
                To = new string('a', mockEmailMaxLength) + "@email.com",
                Source = EmailDataSource,
                Subject = "Subject",
                Parameters = "Body",
                EmailType = "Submitted"
            };

            _mockProviderService.UponReceiving($"Request with 'Email' more than {mockEmailMaxLength} characters")
                .With(TestFactory.CreateAuthorizedPostDynamicEmailRequest(request))
                .WillRespondWith(TestFactory.CreateApiErrorResponse(HttpStatusCode.BadRequest, new[] { tooLongEmailMessage }));

            var service = TestFactory.CreateGenericEmailServiceClient(_settings, _loggerFactory);
            var ex = await Assert.ThrowsAsync<RestClientApiException>(() => service.SendDynamicEmail(request));
            var result = JsonConvert.DeserializeObject<ApiResponse>(ex.ResponseContent);

            Assert.Equal(FailedStatus, result.Status);
            Assert.Equal(FailedStatusMessage, result.Message);
            Assert.Single(result.Errors);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.Errors.First().Code);
            Assert.Single(result.Errors.First().Messages);
        }

        [Fact]
        public async Task GivenVersionRequest_WhenExecuted_ThenReturnsStringVersion()
        {
            const string mockVersion = "1";

            _mockProviderService.UponReceiving("Version request")
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Get,
                    Path = Prefix + GenericEmailServiceClient.GenericEmailServiceVersionUrl
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Headers = new Dictionary<string, object>
                    {
                        {"Content-Type", "application/json"},
                    },
                    Status = 200,
                    Body = mockVersion
                });

            var service = TestFactory.CreateGenericEmailServiceClient(_settings, _loggerFactory);
            var version = await service.GetVersion();

            Assert.Equal(mockVersion, version);
        }

        [Theory]
        [InlineData(null, "OnlineAccountApplication", "Subject", "Body", "Submitted")]
        [InlineData("address@email.com", null, "Subject", "Body", "Submitted")]
        [InlineData("address@email.com", "OnlineAccountApplication", null, "Body", "Submitted")]
        [InlineData("address@email.com", "OnlineAccountApplication", "Subject", null, "Submitted")]
        public async Task GivenSendOnBehalfOfServiceEmailMethod_WhenOneOfRequiredFieldIsEmpty_ThenReturnBadRequestWithOneValidationError(string to, string source, string subject, string body, string emailType)
        {
            var request = new GenericEmailRequest
            {
                To = to,
                Source = source,
                Subject = subject,
                Body = body,
                EmailType = emailType
            };

            var emptyProperty = request.GetType().GetProperties().First(pi => pi.GetValue(request) == null).Name;
            var validationMessage = $"The {emptyProperty} field is required.";

            _mockProviderService.UponReceiving($"Request with 1 empty required field '{emptyProperty}' for send on behalf of service email")
                .With(TestFactory.CreateAuthorizedPostSendOnBehalfOfServiceEmailRequest(request))
                .WillRespondWith(TestFactory.CreateApiErrorResponse(HttpStatusCode.BadRequest, new[] { validationMessage }));

            var service = TestFactory.CreateGenericEmailServiceClient(_settings, _loggerFactory);
            var ex = await Assert.ThrowsAsync<RestClientApiException>(() => service.SendOnBehalfOfServiceEmail(request));
            var result = JsonConvert.DeserializeObject<ApiResponse>(ex.ResponseContent);

            Assert.Equal(FailedStatus, result.Status);
            Assert.Equal(FailedStatusMessage, result.Message);
            Assert.Single(result.Errors);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.Errors.First().Code);
            Assert.Single(result.Errors.First().Messages);
            Assert.Equal(validationMessage, result.Errors.First().Messages.First());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _loggerFactory.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}