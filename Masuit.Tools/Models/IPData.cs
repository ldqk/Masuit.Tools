namespace Masuit.Tools.Models
{
    public class IPData
    {
        /// <summary>
        /// 国家
        /// </summary>
        public string country { get; set; } = "中国";
        /// <summary>
        /// 国家id
        /// </summary>
        public string country_id { get; set; } = "CN";
        /// <summary>
        /// 地区
        /// </summary>
        public string area { get; set; }
        /// <summary>
        /// 地区id
        /// </summary>
        public string area_id { get; set; }
        /// <summary>
        /// 省份
        /// </summary>
        public string region { get; set; }
        /// <summary>
        /// 省份id
        /// </summary>
        public string region_id { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        public string city { get; set; }
        /// <summary>
        /// 市id
        /// </summary>
        public string city_id { get; set; }
        /// <summary>
        /// 区
        /// </summary>
        public string county { get; set; }
        /// <summary>
        /// 区id
        /// </summary>
        public string county_id { get; set; }
        /// <summary>
        /// 网络供应商
        /// </summary>
        public string isp { get; set; }
        /// <summary>
        /// 网络供应商id
        /// </summary>
        public string isp_id { get; set; }
        /// <summary>
        /// ip地址
        /// </summary>
        public string ip { get; set; }
    }
}