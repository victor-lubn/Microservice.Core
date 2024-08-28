using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lueben.Microservice.Options.Options;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lueben.Microservice.Options.OptionManagers
{
    public class RefreshedOptionsManager : RefreshedOptionsManagerWithNoRefresher, IDisposable
    {
        private readonly Timer _timer;
        private readonly ILogger<RefreshedOptionsManager> _logger;
        private bool _disposed;

        public RefreshedOptionsManager(
            IConfigurationRefresherProvider refresherProvider,
            IServiceProvider serviceProvider,
            ILogger<RefreshedOptionsManager> logger,
            IOptions<AppConfigurationRefreshOptions> appConfigurationRefreshOptions)
            : base(serviceProvider)
        {
            _ = refresherProvider ?? throw new ArgumentNullException(nameof(refresherProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (refresherProvider.Refreshers.Any())
            {
                _timer = new Timer(
                    async options => await Task.WhenAll(refresherProvider.Refreshers.Select(RefreshAsync)),
                    null,
                    appConfigurationRefreshOptions?.Value?.DueTime ?? TimeSpan.FromSeconds(30),
                    appConfigurationRefreshOptions?.Value?.Period ?? TimeSpan.FromSeconds(30));
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _timer?.Dispose();
            }

            _disposed = true;
        }

        private async Task RefreshAsync(IConfigurationRefresher refresher)
        {
            var isSuccess = await refresher.TryRefreshAsync();

            if (!isSuccess)
            {
                _logger.LogError($"Unsuccessfully executed configuration refreshing for {refresher.AppConfigurationEndpoint}.");
            }
            else
            {
                _logger.LogInformation($"Successfully executed configuration refreshing for {refresher.AppConfigurationEndpoint}.");
            }
        }
    }
}