using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lueben.Microservice.RetryPolicy;
using Xunit;

namespace Lueben.Microservice.RestSharpClient.RetryPolicy.Tests
{
    public class ExtensionsTests
    {
        private const string Settings = @"{
            ""RetryPolicyOptions:Exception1:MaxRetryCount"": 1,
            ""RetryPolicyOptions:Exception2:MaxRetryCount"": 2           
        }";

        private readonly IConfiguration _configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(Settings))).Build();

        public class Exception1 : Exception
        {
        }

        public class Exception2 : Exception
        {
        }

        public static IEnumerable<object[]> TestExceptions()
        {
            yield return new object[] { new Exception1(), 1 };
            yield return new object[] { new Exception2(), 2 };
            yield return new object[] { new Exception(), new RetryPolicyOptions().MaxRetryCount };
        }

        [Theory]
        [MemberData(nameof(TestExceptions))]
        public void GivenRetryPolicyOptions_WhenExceptionOfSpecificType_ThenCorrespondingConfiguredRetryCountIsUsed<TException>(TException exception, int n) 
            where TException : Exception
        {
            var services = new ServiceCollection();
            services
                .AddTransient(p => _configuration)
                .AddRetryPolicy()
                .AddRetryPolicy<TException>();

            var exceptionType = exception.GetType();
            var serviceProvider = services.BuildServiceProvider();
            var retryPolicy = (RetryPolicy<TException>)serviceProvider.GetService<IRetryPolicy<TException>>();

            Assert.Equal(n, retryPolicy.RetryCount);
        }

        [Fact]
        public void GivenRetryPolicyOptions_WhenNoSettings_ThenDefaultIsUsed()
        {
            var services = new ServiceCollection();
            services
                .AddTransient<IConfiguration>(p => new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes("{}"))).Build())
                .AddRetryPolicy()
                .AddRestClientRetryPolicy();

            var serviceProvider = services.BuildServiceProvider();
            var retryPolicy = (RetryPolicy<RestClientApiException>)serviceProvider.GetService<IRetryPolicy<RestClientApiException>>();

            Assert.NotNull(retryPolicy);
            Assert.Equal(new RetryPolicyOptions().MaxRetryCount, retryPolicy.RetryCount);
        }
    }
}