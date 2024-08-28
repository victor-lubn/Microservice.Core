using System;

namespace Lueben.Microservice.Options.Options
{
    public class AppConfigurationRefreshOptions
    {
        public TimeSpan? DueTime { get; set; }

        public TimeSpan? Period { get; set; }
    }
}
