using Newtonsoft.Json;

namespace Masuit.Tools.Models
{
    /// <summary>
    /// 参考位置
    /// </summary>
    public class Pois
    {
        /// <summary>
        /// 地理位置详细
        /// </summary>
        [JsonProperty("addr")]
        public string AddressDetail { get; set; }

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

        /// <summary>
        /// 建筑物名字
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 建筑物类型
        /// </summary>
        [JsonProperty("poiType")]
        public string PoiType { get; set; }

        /// <summary>
        /// 经纬度
        /// </summary>
        [JsonProperty("point")]
        public LatiLongitude Point { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        [JsonProperty("tag")]
        public string Tag { get; set; }
    }
}