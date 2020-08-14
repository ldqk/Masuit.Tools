using System;
using System.Runtime.Serialization;

namespace Masuit.Tools.Mapping.Exceptions
{
    /// <summary>
    /// 无法执行任何操作时的异常
    /// </summary>
    /// <seealso cref="MapperExceptionBase" />
    [Serializable]
    public class NoActionAfterMappingException : MapperExceptionBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public NoActionAfterMappingException() : base("无法执行操作，因为操作未定义")
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serializer">序列化信息</param>
        /// <param name="context">上下文</param>
        protected NoActionAfterMappingException(SerializationInfo serializer, StreamingContext context) : base(serializer, context)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="exceptionMessage">异常信息</param>
        public NoActionAfterMappingException(string exceptionMessage) : base(exceptionMessage)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="exceptionMessage">异常信息</param>
        /// <param name="innerException">内部异常</param>
        public NoActionAfterMappingException(string exceptionMessage, Exception innerException) : base(exceptionMessage, innerException)
        {
        }
    }
}