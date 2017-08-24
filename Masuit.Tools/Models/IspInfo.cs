using Newtonsoft.Json;

namespace Masuit.Tools.Models
{
    public class IspInfo
    {
        /// <summary>
        /// 运营商
        /// </summary>
        [JsonProperty("wl")]
        public string ISPName { get; set; }
    }
}