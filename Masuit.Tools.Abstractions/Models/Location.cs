using Newtonsoft.Json;

namespace Masuit.Tools.Models
{
    /// <summary>
    /// 经纬度
    /// </summary>
    public class Location
    {
        /// <summary>
        /// 经度
        /// </summary>
        [JsonProperty("lng")]
        public double Lng { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        [JsonProperty("lat")]
        public double Lat { get; set; }
    }
}