using System;
using System.Runtime.Serialization;

namespace Masuit.Tools.Mapping.Exceptions
{
    /// <summary>
    /// 当属性不是同一类型或找不到映射器时出现异常
    /// </summary>
    [Serializable]
    public class NotSameTypePropertyException : MapperExceptionBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="typeSource">源类型</param>
        /// <param name="typeDest">目标类型</param>
        public NotSameTypePropertyException(Type typeSource, Type typeDest) : base(ValideParameter($"源类型{typeSource.Name}目标和类型{typeDest.Name}的属性不是同一类型或找不到映射关系", typeSource != null, typeDest != null))
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public NotSameTypePropertyException()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serializer">序列化信息</param>
        /// <param name="context">上下文</param>
        protected NotSameTypePropertyException(SerializationInfo serializer, StreamingContext context) : base(serializer, context)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="exceptionMessage">异常信息</param>
        /// <param name="innerException">内部异常</param>
        public NotSameTypePropertyException(string exceptionMessage, Exception innerException) : base(exceptionMessage, innerException)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public NotSameTypePropertyException(string exceptionMessage) : base(exceptionMessage)
        {
        }
    }
}