using System;
using System.Runtime.Serialization;

namespace Masuit.Tools.Mapping.Exceptions
{
    /// <summary>
    /// 映射已存在时的异常
    /// </summary>
    [Serializable]
    public class MapperExistException : MapperExceptionBase

    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="source">源类型</param>
        /// <param name="dest">目标类型</param>
        public MapperExistException(Type source, Type dest) : base(ValideParameter($"对于源“{source.FullName}”的类型和目标类型“{dest.FullName}”的映射关系已经存在", source != null, dest != null))
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MapperExistException()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="exceptionMessage">异常信息</param>
        public MapperExistException(string exceptionMessage) : base(exceptionMessage)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serializer">序列化信息</param>
        /// <param name="context">上下文</param>
        protected MapperExistException(SerializationInfo serializer, StreamingContext context) : base(serializer, context)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="exceptionMessage">异常信息</param>
        /// <param name="innerException">内部异常</param>
        public MapperExistException(string exceptionMessage, Exception innerException) : base(exceptionMessage, innerException)
        {
        }
    }
}