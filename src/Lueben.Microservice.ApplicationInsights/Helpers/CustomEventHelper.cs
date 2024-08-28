using System.Collections.Generic;
using System.Linq;
using Lueben.ApplicationInsights;

namespace Lueben.Microservice.ApplicationInsights.Helpers
{
    public static class CustomEventHelper
    {
        public static void RenameEvents<TValue>(IDictionary<string, TValue> telemetryEvents, IList<string> customEvents)
        {
            if (customEvents == null || customEvents.Count == 0)
            {
                return;
            }

            if (telemetryEvents == null)
            {
                return;
            }

            var eventsToRename = telemetryEvents.Where(p => !p.Key.StartsWith(Constants.CompanyPrefix));
            foreach (var (eventToRename, value) in eventsToRename)
            {
                var originalPropName = FunctionPropertyHelper.GetOriginalPropertyName(eventToRename);

                if (!customEvents.Contains(originalPropName))
                {
                    continue;
                }

                var newPropName = PropertyHelper.GetCustomDataPropertyName(originalPropName);
                if (telemetryEvents.TryAdd(newPropName, value))
                {
                    telemetryEvents.Remove(eventToRename);
                }
            }
        }
    }
}
