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
                NewExpression exp = Expression.New(typeObject);
                LambdaExpression lambda = Expression.Lambda(exp);
                Delegate constructor = lambda.Compile();
                defaultValue = constructor.DynamicInvoke();
            }
            if (typeObject.IsEnum)
            {
                defaultValue = Enum.Parse(typeObject, Enum.GetNames(typeObject)[0]);
            }

            return defaultValue;
        }
    }
}