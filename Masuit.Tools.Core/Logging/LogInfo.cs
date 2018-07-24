using System;

namespace Masuit.Tools.Core.Logging
{
    public class LogInfo
    {
        public DateTime Time { get; set; }
        public int ThreadId { get; set; }
        public LogLevel LogLevel { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
    }

    public class LogError : LogInfo
    {
        public Exception Exception { get; set; }
        public string ExceptionType { get; set; }
    }
}