using Microsoft.AspNetCore.Mvc;

namespace Masuit.Tools.AspNetCore.ModelBinder
{
    /// <summary>
    /// 自动装配声明参数值
    /// <list type="bullet">
    /// <item>
    /// 布尔
    /// <description>bool</description>
    /// </item>
    /// <item>
    /// 字符/字符串
    /// <description>char/string</description>
    /// </item>
    /// <item>
    /// 浮点型
    /// <description>float/double/decimal</description>
    /// </item>
    /// <item>
    /// 整型
    /// <description>byte/sbyte/short/ushort/int/uint/long/ulong</description>
    /// </item>
    /// <item>
    /// 枚举
    /// <description>Enum</description>
    /// </item>
    /// <item>
    /// 其他类型
    /// <description>JObject/XDocument/Uri/Guid/TimeSpan</description>
    /// </item>
    /// </list>
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromBodyOrDefaultAttribute(BindType type, string fieldname, IConvertible defaultValue) : ModelBinderAttribute(typeof(FromBodyOrDefaultModelBinder))
    {
        public FromBodyOrDefaultAttribute() : this(BindType.Default, null, null)
        {
        }

        public FromBodyOrDefaultAttribute(IConvertible defaultValue) : this(BindType.Default, null, defaultValue)
        {
        }

        public FromBodyOrDefaultAttribute(BindType type) : this(type, null, null)
        {
        }

        public FromBodyOrDefaultAttribute(BindType type, string fieldname) : this(type, fieldname, null)
        {
        }

        public string FieldName { get; set; } = fieldname;

        public IConvertible DefaultValue { get; set; } = defaultValue;

        /// <summary>
		/// 取值方式
		/// </summary>
		public BindType Type { get; set; } = type;
    }
}
