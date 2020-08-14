using Newtonsoft.Json;

namespace Masuit.Tools.Models
{
    /// <summary>
    /// 详细地理信息
    /// </summary>
    public class PhysicsAddress
    {
        /// <summary>
        /// 返回状态，0正常，1出错
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }

        /// <summary>
        /// 返回结果集
        /// </summary>
        [JsonProperty("result")]
        public AddressResult AddressResult { get; set; } = new AddressResult();
    }
}