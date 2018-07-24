namespace Masuit.Tools.Logging
{
    public class WebLogInfo : LogError
    {
        public string RequestUrl { get; set; }
        public string UserAgent { get; set; }
    }
}