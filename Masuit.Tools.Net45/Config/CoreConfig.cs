using System.Configuration;

namespace Masuit.Tools.Config
{
    public static class ConfigHelper
    {
        public static string GetConfigOrDefault(string key, string defaultValue = "")
        {
            return ConfigurationManager.AppSettings.Get(key) ?? defaultValue;
        }
    }
}
