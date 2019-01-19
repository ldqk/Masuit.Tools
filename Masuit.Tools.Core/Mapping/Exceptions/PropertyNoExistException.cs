using System;
using System.Runtime.Serialization;

namespace Masuit.Tools.Mapping.Exceptions
{
    /// <summary>
    /// 找不到属性时出现异常
    /// </summary>
    [Serializable]
    public class PropertyNoExistException : MapperExceptionBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="typeObject">对象类型</param>
        public PropertyNoExistException(string propertyName, Type typeObject) : base(ValideParameter($"类型“{typeObject}”不存在属性“{propertyName}”", !String.IsNullOrEmpty(propertyName), typeObject != null))
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public PropertyNoExistException()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="exceptionMessage">异常信息</param>
        public PropertyNoExistException(string exceptionMessage) : base(exceptionMessage)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serializer">序列化信息</param>
        /// <param name="context">上下文</param>
        protected PropertyNoExistException(SerializationInfo serializer, StreamingContext context) : base(serializer, context)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="exceptionMessage">异常信息</param>
        /// <param name="innerException">内部异常</param>
        public PropertyNoExistException(string exceptionMessage, Exception innerException) : base(exceptionMessage, innerException)
        {
        }
    }
}