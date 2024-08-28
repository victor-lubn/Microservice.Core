using System.Collections.Generic;

namespace Lueben.Microservice.CircuitBreaker.CleanUp
{
    public class EntityCleanUpOptions
    {
        public string EntityName { get; set; }

        public List<string> Ids { get; set; }

        public bool PurgeWithoutAnalyze { get; set; }

        public EntityCleanUpOptions()
        {
            Ids = new List<string>();
        }
    }
}