using System.Threading;
using System.Threading.Tasks;
using Lueben.Microservice.Serialization;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

#pragma warning disable 618

namespace Lueben.Microservice.Api.PipelineFunction
{
    public class PipelineFunction : IFunctionInvocationFilter
    {
        private const string EnablePipelineKey = "_EnablePipelineKey";

        public PipelineFunction()
        {
        }

        public static bool IsPipelineEnabled(FunctionFilterContext context)
        {
            return context.Properties.ContainsKey(EnablePipelineKey);
        }

        public virtual Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            SetupPipelineFunction();

            executingContext.Properties.Add(EnablePipelineKey, true);
            return Task.CompletedTask;
        }

        public virtual Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs the function setup. It is executed in the first place when <see cref="OnExecutingAsync(FunctionExecutingContext, CancellationToken)"/> method is called.
        /// </summary>
        protected virtual void SetupPipelineFunction()
        {
            JsonConvert.DefaultSettings = FunctionJsonSerializerSettingsProvider.CreateSerializerSettings;
        }
    }
}