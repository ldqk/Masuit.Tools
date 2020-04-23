using Newtonsoft.Json;
using System.Collections.Generic;

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