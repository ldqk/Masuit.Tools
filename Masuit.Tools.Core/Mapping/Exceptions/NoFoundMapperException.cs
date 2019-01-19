using System;
using System.Runtime.Serialization;

namespace Masuit.Tools.Mapping.Exceptions
{
    /// <summary>
    /// 未找到映射关系时出现异常
    /// </summary>
    [Serializable]
    public class NoFoundMapperException : MapperExceptionBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="source">源类型</param>
        /// <param name="dest">目标类型</param>
        public NoFoundMapperException(Type source, Type dest) : base(ValideParameter($"未配置类型“{source.Name}”和“{dest.Name}”的映射", source != null, dest != null))
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">别名</param>
        public NoFoundMapperException(string name) : base(ValideParameter($"找不到名称为{name}的映射", !string.IsNullOrEmpty(name)))
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public NoFoundMapperException()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serializer">序列化信息</param>
        /// <param name="context">上下文</param>
        protected NoFoundMapperException(SerializationInfo serializer, StreamingContext context) : base(serializer, context)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="exceptionMessage">异常信息</param>
        /// <param name="innerException">内部异常</param>
        public NoFoundMapperException(string exceptionMessage, Exception innerException) : base(exceptionMessage, innerException)
        {
        }
    }
}