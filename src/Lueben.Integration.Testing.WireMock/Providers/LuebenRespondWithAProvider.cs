using System;
using System.Collections.Generic;
using WireMock.Models;
using WireMock.ResponseProviders;
using WireMock.Server;
using WireMock.Types;

namespace Lueben.Integration.Testing.WireMock.Providers
{
    public class LuebenRespondWithAProvider : IRespondWithAProvider
    {
        private readonly IRespondWithAProvider _provider;
        private readonly Action _onAfterMappingRegistered;

        public LuebenRespondWithAProvider(IRespondWithAProvider provider, Action onAfterMappingRegistered)
        {
            _provider = provider;
            _onAfterMappingRegistered = onAfterMappingRegistered;
        }

        public Guid Guid { get; }

        public virtual IRespondWithAProvider AtPriority(int priority)
        {
            _provider.AtPriority(priority);
            return this;
        }

        public virtual IRespondWithAProvider InScenario(string scenario)
        {
            _provider.InScenario(scenario);
            return this;
        }

        public virtual IRespondWithAProvider InScenario(int scenario)
        {
            _provider.InScenario(scenario);
            return this;
        }

        public virtual void RespondWith(IResponseProvider provider)
        {
            _provider.RespondWith(provider);

            _onAfterMappingRegistered?.Invoke();
        }

        public virtual IRespondWithAProvider WhenStateIs(string state)
        {
            _provider.WhenStateIs(state);
            return this;
        }

        public virtual IRespondWithAProvider WhenStateIs(int state)
        {
            _provider.WhenStateIs(state);
            return this;
        }

        public virtual IRespondWithAProvider WillSetStateTo(string state, int? times = 1)
        {
            _provider.WillSetStateTo(state, times);
            return this;
        }

        public virtual IRespondWithAProvider WillSetStateTo(int state, int? times = 1)
        {
            _provider.WillSetStateTo(state, times);
            return this;
        }

        public virtual IRespondWithAProvider WithDescription(string description)
        {
            _provider.WithDescription(description);
            return this;
        }

        public virtual IRespondWithAProvider WithGuid(Guid guid)
        {
            _provider.WithGuid(guid);
            return this;
        }

        public virtual IRespondWithAProvider WithGuid(string guid)
        {
            _provider.WithGuid(guid);
            return this;
        }

        public virtual IRespondWithAProvider WithPath(string path)
        {
            _provider.WithGuid(path);
            return this;
        }

        public virtual IRespondWithAProvider WithTimeSettings(ITimeSettings timeSettings)
        {
            _provider.WithTimeSettings(timeSettings);
            return this;
        }

        public virtual IRespondWithAProvider WithTitle(string title)
        {
            _provider.WithTitle(title);
            return this;
        }

        public virtual IRespondWithAProvider WithWebhook(params IWebhook[] webhooks)
        {
            _provider.WithWebhook(webhooks);
            return this;
        }

        public virtual IRespondWithAProvider WithWebhook(
            string url,
            string method = "post",
            IDictionary<string, WireMockList<string>> headers = null,
            string body = null,
            bool useTransformer = true,
            TransformerType transformerType = TransformerType.Handlebars)
        {
            _provider.WithWebhook(url, method, headers, body, useTransformer, transformerType);
            return this;
        }

        public virtual IRespondWithAProvider WithWebhook(
            string url,
            string method = "post",
            IDictionary<string, WireMockList<string>> headers = null,
            object body = null,
            bool useTransformer = true,
            TransformerType transformerType = TransformerType.Handlebars)
        {
            _provider.WithWebhook(url, method, headers, body, useTransformer, transformerType);
            return this;
        }

        public virtual IRespondWithAProvider WithWebhookFireAndForget(bool useWebhooksFireAndForget)
        {
            _provider.WithWebhookFireAndForget(useWebhooksFireAndForget);
            return this;
        }
    }
}
