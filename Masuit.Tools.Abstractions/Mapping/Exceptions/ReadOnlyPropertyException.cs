using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Masuit.Tools.Mapping.Exceptions
{
    /// <summary>
    /// 只读属性的异常
    /// </summary>
    [Serializable]
    public class ReadOnlyPropertyException : MapperExceptionBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="property">属性</param>
        public ReadOnlyPropertyException(PropertyInfo property) : base(ValidateParameter(property))
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ReadOnlyPropertyException()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serializer">序列化信息</param>
        /// <param name="context">上下文</param>
        protected ReadOnlyPropertyException(SerializationInfo serializer, StreamingContext context) : base(serializer, context)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="exceptionMessage">异常信息</param>
        /// <param name="innerException">内部异常</param>
        public ReadOnlyPropertyException(string exceptionMessage, Exception innerException) : base(exceptionMessage, innerException)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="exceptionMessage">异常信息</param>
        public ReadOnlyPropertyException(string exceptionMessage) : base(exceptionMessage)
        {
        }

        private static string ValidateParameter(PropertyInfo property)
        {
            return $"目标对象的属性 '{property.Name}' 是只读的";
        }
    }
}