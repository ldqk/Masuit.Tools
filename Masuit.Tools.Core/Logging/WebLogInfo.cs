namespace Masuit.Tools.Core.Logging
{
    public class WebLogInfo : LogError
    {
        public string RequestUrl { get; set; }
        public string UserAgent { get; set; }
    }
}