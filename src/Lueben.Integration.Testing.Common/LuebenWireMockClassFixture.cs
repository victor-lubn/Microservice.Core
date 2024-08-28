using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Lueben.Integration.Testing.WireMock;
using WireMock.Settings;

namespace Lueben.Integration.Testing.Common
{
    [SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "Overengineering for this particular case, since class doesn't hold any unmanaged resources directly")]
    public class LuebenWireMockClassFixture : IDisposable
    {
        private readonly WireMockServerSettings _serverSettings;
        private LuebenWireMockServer _server;

        private bool _disposed;

        public LuebenWireMockClassFixture()
        {
            _serverSettings = new WireMockServerSettings();
        }

        public string PactDir { get; private set; } = "./";

        public LuebenWireMockClassFixture SetPactsDirectory(string pactDir)
        {
            Directory.CreateDirectory(pactDir);
            PactDir = pactDir;

            return this;
        }

        public LuebenWireMockServer InitializeServer(Action<WireMockServerSettings> configureAction = null, string consumer = null)
        {
            if (_server is null)
            {
                configureAction?.Invoke(_serverSettings);

                _server = LuebenWireMockServer.Start(_serverSettings);

                if (!string.IsNullOrEmpty(consumer))
                {
                    _server.WithConsumer(consumer);
                }
            }

            return _server;
        }

        public void Dispose()
        {
            if (_disposed || _server is null)
            {
                return;
            }

            SavePacts();

            _server.Stop();
            _server.Dispose();

            _disposed = true;
        }

        private void SavePacts()
        {
            _server.SavePact(PactDir);
        }
    }
}
