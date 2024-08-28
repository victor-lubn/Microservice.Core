namespace Lueben.Microservice.DurableFunction.CleanUp
{
    public class DurableFunctionHistoryCleanUpOptions
    {
        public int HistoryExpirationDays { get; set; } = 30;

        public int PurgeHistoryBatchTimeFrameHours { get; set; } = 24;

        public int MaxHistoryAgeMonths { get; set; } = 24;
    }
}
