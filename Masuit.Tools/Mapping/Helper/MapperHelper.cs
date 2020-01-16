using System;
using System.Linq.Expressions;

namespace Masuit.Tools.Mapping.Helper
{
    /// <summary>
    /// mapper帮助类
    /// </summary>
    internal static class MapperHelper
    {
        /// <summary>
        /// 获取类型的默认值。
        /// </summary>
        /// <param name="typeObject">对象类型</param>
        internal static object GetDefaultValue(Type typeObject)
        {
            object defaultValue = null;
            // 对于值类型（例如Integer），必须将对象实例化为具有其默认值。
            if (typeObject.BaseType == typeof(ValueType))
            {
                defaultValue = Expression.Lambda(Expression.New(typeObject)).Compile().DynamicInvoke();
            }
            else if (typeObject.IsEnum)
            {
                defaultValue = Enum.Parse(typeObject, Enum.GetNames(typeObject)[0]);
            }

            return defaultValue;
        }
    }
}