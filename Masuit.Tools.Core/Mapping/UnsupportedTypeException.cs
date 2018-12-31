using System;

namespace Masuit.Tools
{
    /// <summary>
    /// 对尚不支持的类型进行拷贝时抛出的异常
    /// </summary>
    public class UnsupportedTypeException : Exception
    {
        /// <summary>
        /// 用指定的类型初始化 DeepCopier.UnsupportedTypeException 类的新实例
        /// </summary>
        /// <param name="type">暂不支持的类型信息</param>
        public UnsupportedTypeException(Type type) : base($"类型 [{type.Name}] 尚未实现支持！")
        {
        }
    }
}