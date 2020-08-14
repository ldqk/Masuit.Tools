using System;
using System.Runtime.Serialization;

namespace Masuit.Tools.Mapping.Exceptions
{
    /// <summary>
    /// 映射未初始化时的异常
    /// </summary>
    [Serializable]
    public class MapperNotInitializedException : MapperExceptionBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="typeSource">源类型</param>
        /// <param name="typeDest">目标类型</param>
        public MapperNotInitializedException(Type typeSource, Type typeDest) : base(ValideParameter($"源类型“{typeSource.FullName}”和目标类型“{typeDest.FullName}”的映射关系未被初始化，需要在此之前调用ExpressionMapper.Initialize()", typeSource != null, typeDest != null))
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MapperNotInitializedException()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="exceptionMessage">异常信息</param>
        public MapperNotInitializedException(string exceptionMessage) : base(exceptionMessage)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serializer">序列化信息</param>
        /// <param name="context">上下文</param>
        protected MapperNotInitializedException(SerializationInfo serializer, StreamingContext context) : base(serializer, context)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="exceptionMessage">异常信息</param>
        /// <param name="innerException">内部异常</param>
        public MapperNotInitializedException(string exceptionMessage, Exception innerException) : base(exceptionMessage, innerException)
        {
        }
    }
}