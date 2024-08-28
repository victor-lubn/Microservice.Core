using System;
using System.Collections.Generic;
using AutoFixture;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Options;
using Xunit;

namespace Lueben.ApplicationInsights.Exceptions.Tests
{
    public class ExceptionLowLevelTelemetryInitializerTests
    {
        private readonly IFixture _fixture;

        public ExceptionLowLevelTelemetryInitializerTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        public static IEnumerable<object[]> TestData()
        {
            yield return new object[]
                { new TestException() };
            yield return new object[]
                { new DerivedTestException() };
            yield return new object[]
            {
                new Exception("Test exception", new DerivedTestException())
            };
            yield return new object[]
            {
                new Exception("Test exception", new TestException())
            };
            yield return new object[]
            {
                new Exception("Test exception", new Exception("Another test exception", new TestException()))
            };
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void GivenTelemetryInitialize_WhenTelemetryHasConfiguredExceptionsWithErrorSeverityLevel_ThenTelemetryHasInformationSeverityLevel(
            Exception currentException)
        {
            var options = Options.Create(new ExceptionsLowLevelLogOptions
            {
                Exceptions = new Dictionary<SeverityLevel, IEnumerable<string>>
                {
                    {
                        SeverityLevel.Information, new List<string>{ nameof(TestException) }
                    }
                }
            });

            var initializer = new ExceptionLowLevelTelemetryInitializer(options);

            var trace = new ExceptionTelemetry(currentException)
            {
                SeverityLevel = SeverityLevel.Error
            };

            initializer.Initialize(trace);

            Assert.Equal(SeverityLevel.Information, trace.SeverityLevel);
        }

        [Theory]
        [InlineData(SeverityLevel.Error, SeverityLevel.Information, SeverityLevel.Information)]
        [InlineData(SeverityLevel.Error, SeverityLevel.Warning, SeverityLevel.Warning)]
        [InlineData(SeverityLevel.Warning, SeverityLevel.Information, SeverityLevel.Information)]
        [InlineData(SeverityLevel.Information, SeverityLevel.Warning, SeverityLevel.Information)]
        public void GivenTelemetryInitialize_WhenTelemetryHasConfiguredExceptions_ThenTelemetryHasProperSeverityLevel(
            SeverityLevel currentSeverityLevel,
            SeverityLevel configuredSeverityLevel,
            SeverityLevel resultSeverityLevel)
        {
            var options = Options.Create(new ExceptionsLowLevelLogOptions
            {
                Exceptions = new Dictionary<SeverityLevel, IEnumerable<string>>
                {
                    {
                        configuredSeverityLevel, new List<string>{ nameof(TestException) }
                    }
                }
            });

            var initializer = new ExceptionLowLevelTelemetryInitializer(options);

            var trace = new ExceptionTelemetry(new TestException())
            {
                SeverityLevel = currentSeverityLevel
            };

            initializer.Initialize(trace);

            Assert.Equal(resultSeverityLevel, trace.SeverityLevel);
        }

        [Fact]
        public void GivenTelemetryInitialize_WhenTelemetryHasNotConfiguredExceptions_ThenTelemetryNotOverrideSeverityLevel()
        {
            var options = Options.Create(new ExceptionsLowLevelLogOptions
            {
                Exceptions = new Dictionary<SeverityLevel, IEnumerable<string>>
                {
                    {
                        SeverityLevel.Information, new List<string>{ nameof(TestException), "", null }
                    }
                }
            });

            var initializer = new ExceptionLowLevelTelemetryInitializer(options);

            var trace = new ExceptionTelemetry(new Exception("Test Exception"))
            {
                SeverityLevel = SeverityLevel.Error
            };

            initializer.Initialize(trace);

            Assert.Equal(SeverityLevel.Error, trace.SeverityLevel);
        }
    }
}
