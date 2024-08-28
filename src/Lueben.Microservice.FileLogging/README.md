# Description

This package provides a service that allows you to write log to a local file.
The package uses **Serilog** as the main logging provider.
To get more information about the Serilog you can follow the [link](https://serilog.net/)

# Example of FileLogOptions

- LogLevel - (optional) the minimum level of logs. Default value - Information.
- LogFilePath - (optional) local path to log file (used only for local environment). Default value - C:\\Logs.
- LogFileSizeLimitBytes - (optional) the limit of log file in bytes. Default value - 10485760.
- DefaultLogFilesFolder - (required) the path to the folder with log files (used only for release environment). The example of the value of this property for AzureFunction - LogFiles\Application\Functions\Host.