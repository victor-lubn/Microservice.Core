using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Lueben.Integration.Testing.Common
{
    public abstract class HttpFunctionIntegrationTest<TFunction> : BaseFunctionIntegrationTest<TFunction>
    {
        protected HttpFunctionIntegrationTest(
            LuebenWireMockClassFixture wireMockClassFixture,
            ITestOutputHelper testOutputHelper,
            string consumer)
            : base(wireMockClassFixture, testOutputHelper, consumer)
        {
        }

        protected virtual Mock<IHttpContextAccessor> HttpContextAccessorMock { get; set; }

        protected override void SetupServices()
        {
            this.HttpContextAccessorMock = this.Fixture.Freeze<Mock<IHttpContextAccessor>>();
        }

        protected virtual HttpRequest CreateHttpRequest<TBody>(
            IDictionary<string, string> queryParameters = null,
            IDictionary<string, string> headers = null)
            where TBody : class
        {
            var body = this.Fixture.Create<TBody>();
            return this.CreateHttpRequest(body, queryParameters, headers);
        }

        protected virtual HttpRequest CreateHttpRequest(
            object body = null,
            IDictionary<string, string> queryParameters = null,
            IDictionary<string, string> headers = null)
        {
            var context = new DefaultHttpContext();

            this.HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

            if (body != null)
            {
                var json = JsonConvert.SerializeObject(body);
                context.Request.Body = GenerateStreamFromString(json);
            }

            if (queryParameters != null && queryParameters.Any())
            {
                context.Request.QueryString = QueryString.Create(queryParameters);
            }

            if (headers != null && headers.Any())
            {
                foreach (var (headerName, headerValue) in headers)
                {
                    context.Request.Headers[headerName] = headerValue;
                }
            }

            return context.Request;
        }

        private static Stream GenerateStreamFromString(string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
