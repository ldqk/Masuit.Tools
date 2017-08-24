using Newtonsoft.Json;

namespace Masuit.Tools.Models
{
    public class TaobaoIP
    {
        /// <summary>
        /// 返回状态码
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        [JsonProperty("data")]
        public IPData IpData { get; set; }
    }

    public class IPData
    {
        /// <summary>
        /// 国家
        /// </summary>
        [JsonProperty("country")]
        public string Country { get; set; }

        /// <summary>
        /// 省
        /// </summary>
        [JsonProperty("region")]
        public string Region { get; set; }

        /// <summary>
        /// 市
        /// </summary>
        [JsonProperty("city")]
        public string City { get; set; }

        /// <summary>
        /// ISP
        /// </summary>
        [JsonProperty("isp")]
        public string Isp { get; set; }
    }
}