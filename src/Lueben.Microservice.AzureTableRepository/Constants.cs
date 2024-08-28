namespace Lueben.Microservice.AzureTableRepository
{
    public class Constants
    {
        public const string TableNameRegex = "^[A-Za-z][A-Za-z0-9]{2,62}$";

        public const string DefaultTableServiceClientName = nameof(DefaultTableServiceClientName);

        public const string DefaultConnectionName = "AzureWebJobsStorage";

        public const int MaxActionsInTransaction = 100;
    }
}