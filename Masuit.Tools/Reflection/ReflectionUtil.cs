using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Masuit.Tools.Reflection
{
    /// <summary>
    /// 反射操作辅助类，如获取或设置字段、属性的值等反射信息。
    /// </summary>
    public static class ReflectionUtil
    {
        #region 属性字段设置

#pragma warning disable 1591
        public static BindingFlags bf = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
#pragma warning restore 1591

        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="obj">反射对象</param>
        /// <param name="methodName">方法名，区分大小写</param>
        /// <param name="args">方法参数</param>
        /// <typeparam name="T">约束返回的T必须是引用类型</typeparam>
        /// <returns>T类型</returns>
        public static T InvokeMethod<T>(this object obj, string methodName, object[] args) where T : class
        {
            Type type = obj.GetType();
            var objReturn = type.InvokeMember(methodName, bf | BindingFlags.InvokeMethod, null, obj, args) as T;
            return objReturn;
        }

        /// <summary>
        /// 设置字段
        /// </summary>
        /// <param name="obj">反射对象</param>
        /// <param name="name">字段名</param>
        /// <param name="value">值</param>
        public static void SetField(this object obj, string name, object value)
        {
            FieldInfo fi = obj.GetType().GetField(name, bf);
            fi.SetValue(obj, value);
        }

        /// <summary>
        /// 获取字段
        /// </summary>
        /// <param name="obj">反射对象</param>
        /// <param name="name">字段名</param>
        /// <typeparam name="T">约束返回的T必须是引用类型</typeparam>
        /// <returns>T类型</returns>
        public static T GetField<T>(this object obj, string name) where T : class
        {
            FieldInfo fi = obj.GetType().GetField(name, bf);
            return fi.GetValue(obj) as T;
        }

        /// <summary>
        /// 获取所有的字段信息
        /// </summary>
        /// <param name="obj">反射对象</param>
        /// <returns>字段信息</returns>
        public static FieldInfo[] GetFields(this object obj)
        {
            FieldInfo[] fieldInfos = obj.GetType().GetFields(bf);
            return fieldInfos;
        }

        /// <summary>
        /// 设置属性
        /// </summary>
        /// <param name="obj">反射对象</param>
        /// <param name="name">属性名</param>
        /// <param name="value">值</param>
        public static void SetProperty(this object obj, string name, object value)
        {
            PropertyInfo fieldInfo = obj.GetType().GetProperty(name, bf);
            value = Convert.ChangeType(value, fieldInfo.PropertyType);
            fieldInfo.SetValue(obj, value, null);
        }

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="obj">反射对象</param>
        /// <param name="name">属性名</param>
        /// <typeparam name="T">约束返回的T必须是引用类型</typeparam>
        /// <returns>T类型</returns>
        public static T GetProperty<T>(this object obj, string name) where T : class
        {
            PropertyInfo fieldInfo = obj.GetType().GetProperty(name, bf);
            return fieldInfo.GetValue(obj, null) as T;
        }

        /// <summary>
        /// 获取所有的属性信息
        /// </summary>
        /// <param name="obj">反射对象</param>
        /// <returns>属性信息</returns>
        public static PropertyInfo[] GetProperties(this object obj)
        {
            PropertyInfo[] propertyInfos = obj.GetType().GetProperties(bf);
            return propertyInfos;
        }

        #endregion

        #region 获取Description

        /// <summary>
        /// 获取枚举成员的Description信息
        /// </summary>
        /// <param name="value">枚举值</param>
        /// <returns>返回枚举的Description或ToString</returns>
        public static string GetDescription(this Enum value)
        {
            return GetDescription(value, null);
        }

        /// <summary>
        /// 获取枚举值的Description信息
        /// </summary>
        /// <param name ="value">枚举值</param>
        /// <param name ="args">要格式化的对象</param>
        /// <returns>如果未找到DescriptionAttribute则返回null或返回类型描述</returns>
        public static string GetDescription(this Enum value, params object[] args)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            var text1 = attributes.Length > 0 ? attributes[0].Description : value.ToString();
            if ((args != null) && args.Length > 0)
            {
                return string.Format(null, text1, args);
            }

            return text1;
        }

        /// <summary>
        ///	根据成员信息获取Description信息
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <returns>如果未找到DescriptionAttribute则返回null或返回类型描述</returns>
        public static string GetDescription(this MemberInfo member)
        {
            return GetDescription(member, null);
        }

        /// <summary>
        /// 根据成员信息获取Description信息
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <param name="args">格式化占位对象</param>
        /// <returns>如果未找到DescriptionAttribute则返回null或返回类型描述</returns>
        public static string GetDescription(this MemberInfo member, params object[] args)
        {
            string text1;

            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            if (member.IsDefined(typeof(DescriptionAttribute), false))
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])member.GetCustomAttributes(typeof(DescriptionAttribute), false);
                text1 = attributes[0].Description;
            }
            else
            {
                return string.Empty;
            }

            if ((args != null) && (args.Length > 0))
            {
                return string.Format(null, text1, args);
            }

            return text1;
        }

        #endregion

        #region 获取Attribute信息

        /// <summary>
        /// 获取对象的Attributes
        /// </summary>
        /// <param name="attributeType">Type类型</param>
        /// <param name="assembly">程序集信息</param>
        /// <returns></returns>
        public static object GetAttribute(this Type attributeType, Assembly assembly)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException(nameof(attributeType));
            }

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (assembly.IsDefined(attributeType, false))
            {
                object[] attributes = assembly.GetCustomAttributes(attributeType, false);
                return attributes[0];
            }

            return null;
        }

        /// <summary>
        /// 获取对象的Attributes
        /// </summary>
        /// <param name="attributeType">Type类型</param>
        /// <param name="type">成员信息</param>
        /// <returns></returns>
        public static object GetAttribute(this Type attributeType, MemberInfo type)
        {
            return GetAttribute(attributeType, type, false);
        }

        /// <summary>
        /// 获取对象的Attributes
        /// </summary>
        /// <param name="attributeType">Type类型</param>
        /// <param name="type">成员信息</param>
        /// <param name="searchParent">是否在父类中去查找</param>
        /// <returns></returns>
        public static object GetAttribute(this Type attributeType, MemberInfo type, bool searchParent)
        {
            if (type == null)
            {
                return null;
            }

            if (!(attributeType.IsSubclassOf(typeof(Attribute))))
            {
                return null;
            }

            if (type.IsDefined(attributeType, searchParent))
            {
                object[] attributes = type.GetCustomAttributes(attributeType, searchParent);

                if (attributes.Length > 0)
                {
                    return attributes[0];
                }
            }

            return null;
        }

        /// <summary>
        /// 获取对象的Attributes
        /// </summary>
        /// <param name="attributeType">Type类型</param>
        /// <param name="type">成员信息</param>
        /// <returns></returns>
        public static object[] GetAttributes(this Type attributeType, MemberInfo type)
        {
            return GetAttributes(attributeType, type, false);
        }

        /// <summary>
        /// 获取对象的Attributes
        /// </summary>
        /// <param name="attributeType">Type类型</param>
        /// <param name="type">成员信息</param>
        /// <param name="searchParent">是否在父类中去查找</param>
        /// <returns></returns>
        public static object[] GetAttributes(this Type attributeType, MemberInfo type, bool searchParent)
        {
            if (type == null)
            {
                return null;
            }

            if (!(attributeType.IsSubclassOf(typeof(Attribute))))
            {
                return null;
            }

            if (type.IsDefined(attributeType, false))
            {
                return type.GetCustomAttributes(attributeType, searchParent);
            }

            return null;
        }

        #endregion

        #region 资源获取

        /// <summary>
        /// 根据资源名称获取图片资源流
        /// </summary>
        /// <param name="_"></param>
        /// <param name="resourceName">资源的名称</param>
        /// <returns>数据流</returns>
        public static Stream GetImageResource(this Assembly _, string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            return asm.GetManifestResourceStream(resourceName);
        }

        /// <summary>
        /// 获取程序集资源的位图资源
        /// </summary>
        /// <param name="assemblyType">程序集中的某一对象类型</param>
        /// <param name="resourceHolder">资源的根名称。例如，名为“MyResource.en-US.resources”的资源文件的根名称为“MyResource”。</param>
        /// <param name="imageName">资源项名称</param>
        public static Bitmap LoadBitmap(this Type assemblyType, string resourceHolder, string imageName)
        {
            Assembly thisAssembly = Assembly.GetAssembly(assemblyType);
            ResourceManager rm = new ResourceManager(resourceHolder, thisAssembly);
            return (Bitmap)rm.GetObject(imageName);
        }

        /// <summary>
        ///  获取程序集资源的文本资源
        /// </summary>
        /// <param name="assemblyType">程序集中的某一对象类型</param>
        /// <param name="resName">资源项名称</param>
        /// <param name="resourceHolder">资源的根名称。例如，名为“MyResource.en-US.resources”的资源文件的根名称为“MyResource”。</param>
        public static string GetStringRes(this Type assemblyType, string resName, string resourceHolder)
        {
            Assembly thisAssembly = Assembly.GetAssembly(assemblyType);
            ResourceManager rm = new ResourceManager(resourceHolder, thisAssembly);
            return rm.GetString(resName);
        }

        /// <summary>
        /// 获取程序集嵌入资源的文本形式
        /// </summary>
        /// <param name="assemblyType">程序集中的某一对象类型</param>
        /// <param name="charset">字符集编码</param>
        /// <param name="resName">嵌入资源相对路径</param>
        /// <returns>如没找到该资源则返回空字符</returns>
        public static string GetManifestString(this Type assemblyType, string charset, string resName)
        {
            Assembly asm = Assembly.GetAssembly(assemblyType);
            Stream st = asm.GetManifestResourceStream(string.Concat(assemblyType.Namespace, ".", resName.Replace("/", ".")));
            if (st == null)
            {
                return "";
            }

            int iLen = (int)st.Length;
            byte[] bytes = new byte[iLen];
            st.Read(bytes, 0, iLen);
            return (bytes != null) ? Encoding.GetEncoding(charset).GetString(bytes) : "";
        }

        #endregion

        #region 创建对应实例

        /// <summary>
        /// 创建对应实例
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>对应实例</returns>
        public static T CreateInstance<T>(string type) where T : class
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly t in assemblies)
            {
                var tmp = t.GetType(type);
                if (tmp != null)
                {
                    return t.CreateInstance(type) as T;
                }
            }

            return null;
        }

        /// <summary>
        /// 创建对应实例
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>对应实例</returns>
        public static T CreateInstance<T>(this Type type) where T : class
        {
            return CreateInstance<T>(type.FullName);
        }

        #endregion
    }
}