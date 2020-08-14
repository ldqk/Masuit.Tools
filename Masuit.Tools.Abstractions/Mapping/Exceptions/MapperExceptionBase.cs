using System;
using System.Runtime.Serialization;

namespace Masuit.Tools.Mapping.Exceptions
{
    /// <summary>
    /// mapper异常基类
    /// </summary>
    [Serializable]
    public class MapperExceptionBase : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="exceptionMessage">异常信息</param>
        public MapperExceptionBase(string exceptionMessage) : base(exceptionMessage)
        {
        }

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public MapperExceptionBase()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serializer">序列化信息</param>
        /// <param name="context">上下文</param>
        protected MapperExceptionBase(SerializationInfo serializer, StreamingContext context) : base(serializer, context)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="exceptionMessage">异常信息</param>
        /// <param name="innerException">内部异常</param>
        public MapperExceptionBase(string exceptionMessage, Exception innerException) : base(exceptionMessage, innerException)
        {
        }

        /// <summary>
        /// 验证参数
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="conditions">条件</param>
        /// <returns>异常信息</returns>
        protected static string ValideParameter(string message, params bool[] conditions)
        {
            return message;
        }
    }
}