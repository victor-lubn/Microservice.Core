using System;
using DurableTask.Core.Exceptions;
using Lueben.Microservice.DurableFunction.Exceptions;
using Lueben.Microservice.DurableFunction.Extensions;
using Xunit;

namespace Lueben.Microservice.DurableFunction.Tests
{
    public class RetryHandlerTests
    {
        [Fact]
        public void GivenRetryOptionsHandler_WhenExceptionIsNull_ThenExceptionIsNotHandled()
        {
            var retryOptions = DurableOrchestrationContextExtensions.GetRetryOptions(new WorkflowOptions());
            var handled = retryOptions.Handle(null);

            Assert.False(handled);
        }

        [Fact]
        public void GivenRetryOptionsHandler_WhenEventDataProcessFailureException_ThenItIsHandled()
        {
            var retryOptions = DurableOrchestrationContextExtensions.GetRetryOptions(new WorkflowOptions());
            var handled = retryOptions.Handle(new TaskFailedException("test", new EventDataProcessFailureException()));
            Assert.True(handled);
        }

        [Fact]
        public void GivenRetryOptionsHandler_WhenTaskFailedExceptionDoNotHaveInnerException_ThenItIsNotHandled()
        {
            var retryOptions = DurableOrchestrationContextExtensions.GetRetryOptions(new WorkflowOptions());
            var handled = retryOptions.Handle(new TaskFailedException());
            Assert.False(handled);
        }

        [Fact]
        public void GivenRetryOptionsHandler_WhenIncorrectEventDataException_ThenItIsNotHandled()
        {
            var retryOptions = DurableOrchestrationContextExtensions.GetRetryOptions(new WorkflowOptions());
            var handled = retryOptions.Handle(new TaskFailedException("test", new IncorrectEventDataException()));

            Assert.False(handled);
        }

        [Fact]
        public void GivenRetryOptionsHandler_WhenException_ThenItIsNotHandled()
        {
            var retryOptions = DurableOrchestrationContextExtensions.GetRetryOptions(new WorkflowOptions());
            var handled = retryOptions.Handle(new TaskFailedException("test", new Exception()));

            Assert.False(handled);
        }

        [Fact]
        public void GivenWorkflowOptions_WhenWithDefaultValues_ThenConvertedRetryOptionsHaveExpectedValue()
        {
            var retryOptions = DurableOrchestrationContextExtensions.GetRetryOptions(new WorkflowOptions());
           
            Assert.Equal(1, retryOptions.BackoffCoefficient);
        }

        [Fact]
        public void GivenWorkflowOptions_WhenWithSpecifiedValues_ThenConvertedRetryOptionsHaveExpectedValue()
        {
            var retryOptions = DurableOrchestrationContextExtensions.GetRetryOptions(new WorkflowOptions
            {
                BackoffCoefficient = 2,
                ActivityMaxRetryInterval = "P1D",
                ActivityTimeoutInterval = "PT1M"
            });

            Assert.Equal(2, retryOptions.BackoffCoefficient);
            Assert.Equal(TimeSpan.FromDays(1), retryOptions.MaxRetryInterval);
            Assert.Equal(TimeSpan.FromMinutes(1), retryOptions.RetryTimeout);
        }
    }
}