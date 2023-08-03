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
	public class FromBodyOrDefaultAttribute : ModelBinderAttribute
	{
		public FromBodyOrDefaultAttribute() : this(BindType.None, null, null)
		{
		}

		public FromBodyOrDefaultAttribute(IConvertible defaultValue) : this(BindType.None, null, defaultValue)
		{
		}

		public FromBodyOrDefaultAttribute(BindType type, string fieldname) : this(type, fieldname, null)
		{
		}

		public FromBodyOrDefaultAttribute(BindType type, string fieldname, IConvertible defaultValue) : base(typeof(BodyOrDefaultModelBinder))
		{
			Type = type;
			FieldName = fieldname;
			DefaultValue = defaultValue;
		}

		public string FieldName { get; set; }

		public IConvertible DefaultValue { get; set; }

		/// <summary>
		/// 取值方式
		/// </summary>
		public BindType Type { get; set; }
	}

	/// <summary>
	/// 枚举取值方式
	/// </summary>
	public enum BindType
	{
		/// <summary>
		/// 无设定，自动取值(1.取请求数据中的某个值，2.请求数据当成一个对象取值)
		/// </summary>
		None,

		/// <summary>
		/// 从请求正文中获取值
		/// </summary>
		Body,

		/// <summary>
		/// 从查询字符串获取值
		/// </summary>
		Query,

		/// <summary>
		/// 从已发布的表单字段中获取值
		/// </summary>
		Form,

		/// <summary>
		/// 从 HTTP 标头中获取值
		/// </summary>
		Header,

		/// <summary>
		/// 从 Cookie 中取值
		/// </summary>
		Cookie,

		/// <summary>
		/// 从路由数据中获取值
		/// </summary>
		Route,

		/// <summary>
		/// 从依赖关系注入容器中获取类型的实例
		/// </summary>
		Services
	}
}
