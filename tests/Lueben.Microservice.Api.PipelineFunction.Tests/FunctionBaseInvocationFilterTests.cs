using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Executors;
using Xunit;
#pragma warning disable 618

namespace Lueben.Microservice.Api.PipelineFunction.Tests
{
    public class FunctionBaseInvocationFilterTests
    {
        [Fact]
        public void GivenFunctionBaseInvocationFilter_WhenPipelineIsNotEnabled_ThenFilterIsNotExecuted()
        {
            var filter = new TestFilter();

            var properties = new Dictionary<string, object>();
            var executingContext = new FunctionExecutingContext(new Dictionary<string, object>(), properties, Guid.NewGuid(), "function", null);
            var context = new FunctionExecutedContext(new Dictionary<string, object>(), executingContext.Properties, Guid.NewGuid(), "function", null, new FunctionResult(true));

            filter.OnExecutingAsync(executingContext, CancellationToken.None);
            filter.OnExecutedAsync(context, CancellationToken.None);
            Assert.False(filter.Executed);
        }

        [Fact]
        public void GivenFunctionBaseInvocationFilter_WhenPipelineIsEnabled_ThenFilterIsExecuted()
        {
            var filter = new TestFilter();

            var properties = new Dictionary<string, object>();
            var executingContext = new FunctionExecutingContext(new Dictionary<string, object>(), properties, Guid.NewGuid(), "function", null);
            var context = new FunctionExecutedContext(new Dictionary<string, object>(), executingContext.Properties, Guid.NewGuid(), "function", null, new FunctionResult(true));
            var function = new PipelineFunction();
            function.OnExecutingAsync(executingContext, CancellationToken.None);

            filter.OnExecutedAsync(context, CancellationToken.None);
            Assert.True(filter.Executed);
        }
    }
}