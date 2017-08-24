using Newtonsoft.Json;

namespace Masuit.Tools.Models
{
    public class BaiduIP
    {
        /// <summary>
        /// 基本信息
        /// </summary>
        [JsonProperty("address")]
        public string Address { get; set; }

        /// <summary>
        /// 基本地理信息
        /// </summary>
        [JsonProperty("content")]
        public AddressInfo AddressInfo { get; set; }

        /// <summary>
        /// 返回状态，0正常，1出错
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }
    }

    /// <summary>
    /// 基本地理信息
    /// </summary>
    public class AddressInfo
    {
        /// <summary>
        /// 基本地址
        /// </summary>
        [JsonProperty("address")]
        public string Address { get; set; }

        /// <summary>
        /// 经纬度
        /// </summary>
        [JsonProperty("point")]
        public LatiLongitude LatiLongitude { get; set; }
    }

    public class LatiLongitude
    {
        /// <summary>
        /// 经度
        /// </summary>
        [JsonProperty("x")]
        public string X { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        [JsonProperty("y")]
        public string Y { get; set; }
    }
}