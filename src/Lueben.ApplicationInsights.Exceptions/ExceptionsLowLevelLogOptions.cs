using System.Collections.Generic;
using Microsoft.ApplicationInsights.DataContracts;

namespace Lueben.ApplicationInsights.Exceptions
{
    public class ExceptionsLowLevelLogOptions
    {
        public IDictionary<SeverityLevel, IEnumerable<string>> Exceptions { get; set; }
    }
}