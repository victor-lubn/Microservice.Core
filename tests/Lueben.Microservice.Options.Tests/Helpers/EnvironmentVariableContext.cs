using System;

namespace Lueben.Microservice.Options.Tests.Helpers
{
    public class EnvironmentVariableContext : IDisposable
    {
        private readonly string _key;
        private readonly string _previousValue;
        private bool _disposed;

        public EnvironmentVariableContext(string key, string value)
        {
            _key = key;
            _previousValue = Environment.GetEnvironmentVariable(key);
            Environment.SetEnvironmentVariable(key, value);
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
                Environment.SetEnvironmentVariable(_key, _previousValue);
            }

            _disposed = true;
        }
    }
}
