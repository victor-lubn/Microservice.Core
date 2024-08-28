using System;
using System.Collections.Generic;
using System.Diagnostics;
using Azure.Messaging.EventHubs;

namespace Lueben.Microservice.EventHub.Extensions
{
    public static class EventDataDiagnosticExtensions
    {
        public const string ActivityIdPropertyName = "Diagnostic-Id";
        public const string CorrelationContextPropertyName = "Correlation-Context";
        public const string ProcessActivityName = "Process";

        public static Activity ExtractActivity(this EventData eventData, string activityName = null)
        {
            activityName ??= ProcessActivityName;

            var activity = new Activity(activityName);

            if (TryExtractId(eventData, out var id))
            {
                activity.SetParentId(id);

                if (eventData.TryExtractContext(out var ctx))
                {
                    foreach (var kvp in ctx)
                    {
                        activity.AddBaggage(kvp.Key, kvp.Value);
                    }
                }
            }

            return activity;
        }

        internal static bool TryExtractId(this EventData eventData, out string id)
        {
            id = null;
            if (eventData.Properties.TryGetValue(ActivityIdPropertyName, out var requestId))
            {
                if (requestId is string tmp && tmp.Trim().Length > 0)
                {
                    id = tmp;
                    return true;
                }
            }

            return false;
        }

        internal static bool TryExtractContext(this EventData eventData, out IList<KeyValuePair<string, string>> context)
        {
            context = null;
            try
            {
                if (eventData.Properties.TryGetValue(CorrelationContextPropertyName, out object ctxObj))
                {
                    var ctxStr = ctxObj as string;
                    if (string.IsNullOrEmpty(ctxStr))
                    {
                        return false;
                    }

                    var ctxList = ctxStr.Split(',');
                    if (ctxList.Length == 0)
                    {
                        return false;
                    }

                    context = new List<KeyValuePair<string, string>>();
                    foreach (var item in ctxList)
                    {
                        var kvp = item.Split('=');
                        if (kvp.Length == 2)
                        {
                            context.Add(new KeyValuePair<string, string>(kvp[0], kvp[1]));
                        }
                    }

                    return true;
                }
            }
            catch (Exception)
            {
                // ignored, if context is invalid, there is nothing we can do:
                // invalid context was created by producer, but if we throw here, it will break message processing on consumer
                // and consumer does not control which context it receives
            }

            return false;
        }
    }
}
