# Description
Contains the time triggered function that allows to clean up history of the durable function executions.

# Configuration
The following settings should be configured in order to make the cleanUp functionality working:
- DurableFunctionHistoryCleanUpTimerScheduleExpression - function level setting that specifies the frequency of the function run. Contains ncrontab expression, e.g. 0 0 0 * * * (run each day at midnight). Default value is 0 0 1 * * *
- DurableFunctionHistoryCleanUpOptions:HistoryExpirationDays - application setting that specifies the expiration time id days for the records that should be removed from the history tables. Default value is 30 days.
- DurableFunctionHistoryCleanUpOptions:PurgeHistoryBatchTimeFrameHours - Value in hours that allows to split scanning of History table into chuncks of specific time frame. Default value is 24 hours.
- DurableFunctionHistoryCleanUpOptions:MaxHistoryAgeMonths - Value in months that allows to specify max age of the history entities that eligible for scan and purge. Default value is 24 months.
