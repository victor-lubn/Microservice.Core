namespace Lueben.Microservice.FileLogging
{
    public class FileLogOptions
    {
        public FileLogOptions()
        {
            DefaultLogFilesFolder = @"LogFiles\Application\Functions\Host";
        }

        public string LogLevel { get; set; }

        public string LogFilePath { get; set; }

        public int LogFileSizeLimitBytes { get; set; }

        public string DefaultLogFilesFolder { get; set; }
    }
}
