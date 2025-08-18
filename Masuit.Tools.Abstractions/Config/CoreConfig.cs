#if NETFRAMEWORK
using System;
using System.Configuration;
using Newtonsoft.Json;

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

#else

using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Masuit.Tools.Config
{
    /// <summary>
    /// .net core的配置导入
    /// </summary>
    public static class ConfigHelper
    {
        /// <summary>
        /// 配置对象
        /// </summary>
        private static IConfiguration Configuration { get; set; } = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json", true, true).Build();

        public static string GetConfigOrDefault(string key, string defaultValue = "")
        {
            string config = Configuration[key];
            return config.IsNullOrEmpty() ? defaultValue : config;
        }

        public static T Get<T>(string key)
        {
            var section = Configuration?.GetSection(key);
            if (section == null)
            {
                return default;
            }

            if (!section.GetChildren().Any())
            {
                var value = section.Value ?? Configuration[key];
                if (string.IsNullOrEmpty(value))
                {
                    return default;
                }

                return ConvertTo<T>(value);
            }

            var token = BuildToken(section);
            return token is null ? default : token.ToObject<T>(JsonSerializer.CreateDefault());
        }

        private static T ConvertTo<T>(string value)
        {
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            try
            {
                if (targetType == typeof(string))
                {
                    return (T)(object)value;
                }

                if (targetType.IsEnum)
                {
                    return (T)Enum.Parse(targetType, value, true);
                }

                if (targetType == typeof(Guid))
                {
                    return (T)(object)Guid.Parse(value);
                }

                if (targetType == typeof(bool))
                {
                    if (bool.TryParse(value, out var b)) return (T)(object)b;
                    if (int.TryParse(value, out var bi)) return (T)(object)(bi != 0);
                }

                if (targetType.IsPrimitive || targetType == typeof(decimal))
                {
                    return (T)Convert.ChangeType(value, targetType);
                }
            }
            catch
            {
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

        private static JToken BuildToken(IConfigurationSection section)
        {
            var children = section.GetChildren().ToList();
            if (children.Count == 0)
            {
                return section.Value != null ? JToken.FromObject(section.Value) : null;
            }

            if (children.All(c => int.TryParse(c.Key, out _)))
            {
                var array = new JArray();
                foreach (var child in children.OrderBy(c => int.Parse(c.Key)))
                {
                    array.Add(BuildToken(child));
                }
                return array;
            }

            var obj = new JObject();
            foreach (var child in children)
            {
                obj[child.Key] = BuildToken(child);
            }
            return obj;
        }

        /// <summary>
        /// 将配置添加到Masuit.Tools，若未调用，将自动加载默认的appsettings.json
        /// </summary>
        /// <param name="config"></param>
        public static void AddToMasuitTools(this IConfiguration config)
        {
            Configuration = config;
        }
    }
}

#endif