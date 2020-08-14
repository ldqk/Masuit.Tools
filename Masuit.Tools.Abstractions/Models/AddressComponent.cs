using Newtonsoft.Json;

namespace Masuit.Tools.Models
{
    /// <summary>
    /// 地理信息成分
    /// </summary>
    public class AddressComponent
    {
        /// <summary>
        /// 国家
        /// </summary>
        [JsonProperty("country")]
        public string Country { get; set; }

        /// <summary>
        /// 国家代码
        /// </summary>
        [JsonProperty("country_code")]
        public int CountryCode { get; set; }

        /// <summary>
        /// 省
        /// </summary>
        [JsonProperty("province")]
        public string Province { get; set; }

        /// <summary>
        /// 市
        /// </summary>
        [JsonProperty("city")]
        public string City { get; set; }

        /// <summary>
        /// 区
        /// </summary>
        [JsonProperty("district")]
        public string District { get; set; }

        /// <summary>
        /// 街道
        /// </summary>
        [JsonProperty("street")]
        public string Street { get; set; }

        /// <summary>
        /// 门牌号
        /// </summary>
        [JsonProperty("street_number")]
        public string StreetNumber { get; set; }

        /// <summary>
        /// 方位
        /// </summary>
        [JsonProperty("direction")]
        public string Direction { get; set; }

        /// <summary>
        /// 距离
        /// </summary>
        [JsonProperty("distance")]
        public string Distance { get; set; } = "0";
    }
}