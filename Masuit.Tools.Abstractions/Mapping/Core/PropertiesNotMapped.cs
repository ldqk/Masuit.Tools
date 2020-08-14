using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Masuit.Tools.Mapping.Core
{
    /// <summary>
    /// 未映射的属性的处理结果。
    /// </summary>
    public class PropertiesNotMapped
    {

        internal List<PropertyInfo> sourceProperties;
        internal List<PropertyInfo> targetProperties;

        /// <summary>
        /// 获取未映射的源属性。
        /// </summary>
        public ReadOnlyCollection<PropertyInfo> SourceProperties => new ReadOnlyCollection<PropertyInfo>(sourceProperties);

        /// <summary>
        /// 获取未映射的目标属性。
        /// </summary>
        public ReadOnlyCollection<PropertyInfo> TargetProperties => new ReadOnlyCollection<PropertyInfo>(targetProperties);

        /// <summary>
        /// 构造函数
        /// </summary>
        public PropertiesNotMapped()
        {
            sourceProperties = new List<PropertyInfo>();
            targetProperties = new List<PropertyInfo>();
        }
    }
}
