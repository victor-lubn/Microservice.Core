namespace Lueben.Microservice.DurableFunction.HealthCheck
{
    public class WorkflowHealthCheckOptions
    {
        public double MaxDaysSinceLastUpdated { get; set; } = 1;

        public double CreatedDaysFrom { get; set; } = 30;

        public int HistoryPageSize { get; set; } = 100;
    }
}