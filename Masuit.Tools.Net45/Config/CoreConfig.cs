using Newtonsoft.Json;
using System.Configuration;

namespace Masuit.Tools.Config
{
    public static class ConfigHelper
    {
        public static string GetConfigOrDefault(string key, string defaultValue = "")
        {
            return ConfigurationManager.AppSettings.Get(key) ?? defaultValue;
        }

        public static T Get<T>(string key)
        {
            var value = ConfigurationManager.AppSettings.Get(key);
            if (string.IsNullOrWhiteSpace(value))
            {
                return default;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch
            {
                return default;
            }
        }
    }
}