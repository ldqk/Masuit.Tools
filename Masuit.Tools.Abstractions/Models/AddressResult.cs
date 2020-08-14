using Newtonsoft.Json;
using System.Collections.Generic;

namespace Masuit.Tools.Models
{
    /// <summary>
    /// 返回结果集
    /// </summary>
    public class AddressResult
    {
        /// <summary>
        /// 经纬度
        /// </summary>
        [JsonProperty("location")]
        public Location Location { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        [JsonProperty("formatted_address")]
        public string FormattedAddress { get; set; }

        /// <summary>
        /// 商业地址
        /// </summary>
        [JsonProperty("business")]
        public string Business { get; set; }

        /// <summary>
        /// 地理信息成分
        /// </summary>
        [JsonProperty("addressComponent")]
        public AddressComponent AddressComponent { get; set; }

        /// <summary>
        /// 参考地址
        /// </summary>
        [JsonProperty("pois")]
        public List<Pois> Pois { get; set; } = new List<Pois>();

        /// <summary>
        /// 语义描述
        /// </summary>
        [JsonProperty("sematic_description")]
        public string SematicDescription { get; set; }
    }
}