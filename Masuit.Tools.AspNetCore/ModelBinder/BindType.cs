namespace Masuit.Tools.AspNetCore.ModelBinder;

/// <summary>
/// 取值方式
/// </summary>
[Flags]
public enum BindType
{
	/// <summary>
	/// 自动取值(1.取请求数据中的某个值，2.请求数据当成一个对象取值)
	/// </summary>
	Default = Body | Query | Form | Header | Cookie | Route | Services,

	/// <summary>
	/// 从查询字符串获取值
	/// </summary>
	Query = 1,

	/// <summary>
	/// 从请求正文中获取值
	/// </summary>
	Body = 2,

	/// <summary>
	/// 从已发布的表单字段中获取值
	/// </summary>
	Form = 4,

	/// <summary>
	/// 从 HTTP 标头中获取值
	/// </summary>
	Header = 8,

	/// <summary>
	/// 从 Cookie 中取值
	/// </summary>
	Cookie = 16,

	/// <summary>
	/// 从路由数据中获取值
	/// </summary>
	Route = 32,

	/// <summary>
	/// 从依赖关系注入容器中获取类型的实例
	/// </summary>
	Services = 64
}
