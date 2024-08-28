using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.Options.OptionManagers
{
    public class RefreshedOptionsManagerWithNoRefresher : IRefreshedOptionsManager
    {
        private readonly IServiceProvider _serviceProvider;

        public RefreshedOptionsManagerWithNoRefresher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public TOptions GetOptions<TOptions>()
            where TOptions : class, new()
        {
            var options = _serviceProvider.GetService<IOptionsSnapshot<TOptions>>();
            return options?.Value;
        }
    }
}