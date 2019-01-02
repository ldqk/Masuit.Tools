using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Masuit.Tools.Reflection
{
    /// <summary>  
    /// 类帮助器,可以动态对类,类成员进行控制(添加,删除),目前只支持属性控制。  
    /// 注意,属性以外的其它成员会被清空,功能还有待完善,使其不影响其它成员。  
    /// </summary>  
    public static class ClassHelper
    {
        #region 公有方法  

        /// <summary>  
        /// 根据类的类型型创建类实例。  
        /// </summary>  
        /// <param name="t">将要创建的类型。</param>  
        /// <returns>返回创建的类实例。</returns>  
        public static object CreateInstance(this Type t)
        {
            return Activator.CreateInstance(t);
        }


        /// <summary>  
        /// 根据类的名称,属性列表创建型实例。  
        /// </summary>  
        /// <param name="className">将要创建的类的名称。</param>  
        /// <param name="lcpi">将要创建的类的属性列表。</param>  
        /// <returns>返回创建的类实例</returns>  
        public static object CreateInstance(string className, List<CustPropertyInfo> lcpi)
        {
            return Activator.CreateInstance(AddProperty(BuildType(className), lcpi));
        }


        /// <summary>  
        /// 根据属性列表创建类的实例,默认类名为DefaultClass,由于生成的类不是强类型,所以类名可以忽略。  
        /// </summary>  
        /// <param name="lcpi">将要创建的类的属性列表</param>  
        /// <returns>返回创建的类的实例。</returns>  
        public static object CreateInstance(List<CustPropertyInfo> lcpi)
        {
            return CreateInstance("DefaultClass", lcpi);
        }

        /// <summary>  
        /// 创建一个没有成员的类型的实例,类名为"DefaultClass"。  
        /// </summary>  
        /// <returns>返回创建的类型的实例。</returns>  
        public static Type BuildType()
        {
            return BuildType("DefaultClass");
        }


        /// <summary>  
        /// 根据类名创建一个没有成员的类型的实例。  
        /// </summary>  
        /// <param name="className">将要创建的类型的实例的类名。</param>  
        /// <returns>返回创建的类型的实例。</returns>  
        public static Type BuildType(string className)
        {
            AssemblyName myAsmName = new AssemblyName
            {
                Name = "MyDynamicAssembly"
            };

            //创建一个程序集,设置为AssemblyBuilderAccess.RunAndCollect。  
            AssemblyBuilder myAsmBuilder = AssemblyBuilder.DefineDynamicAssembly(myAsmName, AssemblyBuilderAccess.RunAndCollect);

            //创建一个单模程序块。  
            ModuleBuilder myModBuilder = myAsmBuilder.DefineDynamicModule(myAsmName.Name);
            //创建TypeBuilder。  
            TypeBuilder myTypeBuilder = myModBuilder.DefineType(className, TypeAttributes.Public);

            //创建类型。  
            Type retval = myTypeBuilder.CreateType();

            //保存程序集,以便可以被Ildasm.exe解析,或被测试程序引用。  
            //myAsmBuilder.Save(myAsmName.Name + ".dll");  
            return retval;
        }

        /// <summary>  
        /// 添加属性到类型的实例,注意:该操作会将其它成员清除掉,其功能有待完善。  
        /// </summary>  
        /// <param name="classType">指定类型的实例。</param>  
        /// <param name="lcpi">表示属性的一个列表。</param>  
        /// <returns>返回处理过的类型的实例。</returns>  
        public static Type AddProperty(this Type classType, List<CustPropertyInfo> lcpi)
        {
            //合并先前的属性,以便一起在下一步进行处理。  
            MergeProperty(classType, lcpi);
            //把属性加入到Type。  
            return AddPropertyToType(classType, lcpi);
        }


        /// <summary>  
        /// 添加属性到类型的实例,注意:该操作会将其它成员清除掉,其功能有待完善。  
        /// </summary>  
        /// <param name="classType">指定类型的实例。</param>  
        /// <param name="cpi">表示一个属性。</param>  
        /// <returns>返回处理过的类型的实例。</returns>  
        public static Type AddProperty(this Type classType, CustPropertyInfo cpi)
        {
            List<CustPropertyInfo> lcpi = new List<CustPropertyInfo>
            {
                cpi
            };
            //合并先前的属性,以便一起在下一步进行处理。  
            MergeProperty(classType, lcpi);
            //把属性加入到Type。  
            return AddPropertyToType(classType, lcpi);
        }

        /// <summary>
        /// 给对象实例添加新属性并返回新对象实例
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="lcpi">自定义属性对象集合</param>
        /// <returns></returns>
        public static object AddProperty(this object obj, List<CustPropertyInfo> lcpi)
        {
            Type originType = obj.GetType();
            var customs = lcpi.ToDictionary(i => i.PropertyName, i => i.PropertyValue);
            var dest = AddProperty(originType, lcpi).CreateInstance();
            foreach (var originProperty in originType.GetProperties())
            {
                dest.SetProperty(originProperty.Name, originProperty.GetValue(obj));
            }
            foreach (var cpi in customs)
            {
                dest.SetProperty(cpi.Key, cpi.Value);
            }
            return dest;
        }

        /// <summary>
        /// 给对象实例添加新属性并返回新对象实例
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="cpi">自定义属性对象</param>
        /// <returns></returns>
        public static object AddProperty(this object obj, CustPropertyInfo cpi)
        {
            return AddProperty(obj, new List<CustPropertyInfo>() { cpi });
        }

        /// <summary>
        /// 给对象实例添加新属性并返回新对象实例
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        /// <returns></returns>
        public static object AddProperty(this object obj, string propertyName, object propertyValue)
        {
            return AddProperty(obj, new List<CustPropertyInfo>() { new CustPropertyInfo(propertyValue.GetType(), propertyName, propertyValue) });
        }

        /// <summary>  
        /// 从类型的实例中移除属性,注意:该操作会将其它成员清除掉,其功能有待完善。  
        /// </summary>  
        /// <param name="classType">指定类型的实例。</param>  
        /// <param name="propertyName">要移除的属性。</param>  
        /// <returns>返回处理过的类型的实例。</returns>  
        public static Type DeleteProperty(this Type classType, string propertyName)
        {
            List<string> ls = new List<string>
            {
                propertyName
            };

            //合并先前的属性,以便一起在下一步进行处理。  
            List<CustPropertyInfo> lcpi = SeparateProperty(classType, ls);
            //把属性加入到Type。  
            return AddPropertyToType(classType, lcpi);
        }


        /// <summary>  
        /// 从类型的实例中移除属性,注意:该操作会将其它成员清除掉,其功能有待完善。  
        /// </summary>  
        /// <param name="classType">指定类型的实例。</param>  
        /// <param name="propertyNames">要移除的属性列表。</param>  
        /// <returns>返回处理过的类型的实例。</returns>  
        public static Type DeleteProperty(this Type classType, List<string> propertyNames)
        {
            //合并先前的属性,以便一起在下一步进行处理。  
            List<CustPropertyInfo> lcpi = SeparateProperty(classType, propertyNames);
            //把属性加入到Type。  
            return AddPropertyToType(classType, lcpi);
        }

        /// <summary>
        /// 删除对象的属性并返回新对象实例
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyNames">属性名集合</param>
        /// <returns></returns>
        public static object DeleteProperty(this object obj, List<string> propertyNames)
        {
            PropertyInfo[] oldProperties = obj.GetType().GetProperties();
            Type t = obj.GetType();
            foreach (string p in propertyNames)
            {
                t = t.DeleteProperty(p);
            }
            var newInstance = t.CreateInstance();
            foreach (var p in newInstance.GetProperties())
            {
                newInstance.SetProperty(p.Name, oldProperties.FirstOrDefault(i => i.Name.Equals(p.Name)).GetValue(obj));
            }
            return newInstance;
        }

        /// <summary>
        /// 删除对象的属性并返回新对象实例
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="property">属性名</param>
        /// <returns></returns>
        public static object DeleteProperty(this object obj, string property)
        {
            return DeleteProperty(obj, new List<string>() { property });
        }
        #endregion

        #region 私有方法  

        /// <summary>  
        /// 把类型的实例t和lcpi参数里的属性进行合并。  
        /// </summary>  
        /// <param name="t">实例t</param>  
        /// <param name="lcpi">里面包含属性列表的信息。</param>  
        private static void MergeProperty(Type t, List<CustPropertyInfo> lcpi)
        {
            foreach (PropertyInfo pi in t.GetProperties())
            {
                CustPropertyInfo cpi = new CustPropertyInfo(pi.PropertyType, pi.Name);
                lcpi.Add(cpi);
            }
        }


        /// <summary>  
        /// 从类型的实例t的属性移除属性列表lcpi,返回的新属性列表在lcpi中。  
        /// </summary>  
        /// <param name="t">类型的实例t。</param>  
        /// <param name="ls">要移除的属性列表。</param>  
        private static List<CustPropertyInfo> SeparateProperty(Type t, List<string> ls)
        {
            List<CustPropertyInfo> ret = new List<CustPropertyInfo>();
            foreach (PropertyInfo pi in t.GetProperties())
            {
                foreach (string s in ls)
                {
                    if (pi.Name != s)
                    {
                        CustPropertyInfo cpi = new CustPropertyInfo(pi.PropertyType, pi.Name);
                        ret.Add(cpi);
                    }
                }
            }

            return ret;
        }


        /// <summary>  
        /// 把lcpi参数里的属性加入到myTypeBuilder中。注意:该操作会将其它成员清除掉,其功能有待完善。  
        /// </summary>  
        /// <param name="myTypeBuilder">类型构造器的实例。</param>  
        /// <param name="lcpi">里面包含属性列表的信息。</param>  
        private static void AddPropertyToTypeBuilder(TypeBuilder myTypeBuilder, List<CustPropertyInfo> lcpi)
        {
            // 属性Set和Get方法要一个专门的属性。这里设置为Public。  
            var getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            // 添加属性到myTypeBuilder。  
            foreach (CustPropertyInfo cpi in lcpi)
            {
                //定义字段。  
                FieldBuilder customerNameBldr = myTypeBuilder.DefineField(cpi.FieldName, cpi.Type, FieldAttributes.Private);

                //customerNameBldr.SetConstant("11111111");
                //定义属性。  
                //最后一个参数为null,因为属性没有参数。  
                var custNamePropBldr = myTypeBuilder.DefineProperty(cpi.PropertyName, PropertyAttributes.HasDefault, cpi.Type, null);

                //custNamePropBldr.SetConstant("111111111");
                //定义Get方法。  
                var custNameGetPropMthdBldr = myTypeBuilder.DefineMethod(cpi.GetPropertyMethodName, getSetAttr, cpi.Type, Type.EmptyTypes);

                var custNameGetIL = custNameGetPropMthdBldr.GetILGenerator();

                try
                {
                    custNameGetIL.Emit(OpCodes.Ldarg_0);
                    //custNameGetIL.Emit(OpCodes.Ldfld, customerNameBldr);  
                    custNameGetIL.Emit(OpCodes.Ldfld, customerNameBldr);
                    custNameGetIL.Emit(OpCodes.Ret);
                }
                catch (Exception)
                {
                    // ignored
                }

                //定义Set方法。  
                var custNameSetPropMthdBldr = myTypeBuilder.DefineMethod(cpi.SetPropertyMethodName, getSetAttr, null, new[]
                {
                    cpi.Type
                });

                var custNameSetIL = custNameSetPropMthdBldr.GetILGenerator();

                custNameSetIL.Emit(OpCodes.Ldarg_0);
                custNameSetIL.Emit(OpCodes.Ldarg_1);
                custNameSetIL.Emit(OpCodes.Stfld, customerNameBldr);
                custNameSetIL.Emit(OpCodes.Ret);
                //custNamePropBldr.SetConstant("ceshi");  
                //把创建的两个方法(Get,Set)加入到PropertyBuilder中。  
                custNamePropBldr.SetGetMethod(custNameGetPropMthdBldr);
                custNamePropBldr.SetSetMethod(custNameSetPropMthdBldr);
            }
        }


        /// <summary>  
        /// 把属性加入到类型的实例。  
        /// </summary>  
        /// <param name="classType">类型的实例。</param>  
        /// <param name="lcpi">要加入的属性列表。</param>  
        /// <returns>返回处理过的类型的实例。</returns>  
        public static Type AddPropertyToType(this Type classType, List<CustPropertyInfo> lcpi)
        {
            AssemblyName myAsmName = new AssemblyName
            {
                Name = "MyDynamicAssembly"
            };

            //创建一个程序集,设置为AssemblyBuilderAccess.RunAndCollect。  
            AssemblyBuilder myAsmBuilder = AssemblyBuilder.DefineDynamicAssembly(myAsmName, AssemblyBuilderAccess.RunAndCollect);

            //创建一个单模程序块。  
            ModuleBuilder myModBuilder = myAsmBuilder.DefineDynamicModule(myAsmName.Name);
            //创建TypeBuilder。  
            // ReSharper disable once AssignNullToNotNullAttribute
            TypeBuilder myTypeBuilder = myModBuilder.DefineType(classType.FullName, TypeAttributes.Public);

            //把lcpi中定义的属性加入到TypeBuilder。将清空其它的成员。其功能有待扩展,使其不影响其它成员。  
            AddPropertyToTypeBuilder(myTypeBuilder, lcpi);

            //创建类型。  
            Type retval = myTypeBuilder.CreateType();

            //保存程序集,以便可以被Ildasm.exe解析,或被测试程序引用。  
            //myAsmBuilder.Save(myAsmName.Name + ".dll");  
            return retval;
        }

        #endregion

        #region 辅助类  

        /// <summary>  
        /// 自定义的属性信息类型。  
        /// </summary>  
        public class CustPropertyInfo
        {
            /// <summary>  
            /// 空构造。  
            /// </summary>  
            public CustPropertyInfo()
            {
            }

            /// <summary>  
            /// 根据属性类型名称,属性名称构造实例。  
            /// </summary>  
            /// <param name="type">属性类型名称。</param>  
            /// <param name="propertyName">属性名称。</param>  
            public CustPropertyInfo(Type type, string propertyName)
            {
                Type = type;
                PropertyName = propertyName;
            }

            /// <summary>
            /// 根据属性类型名称,属性名称构造实例，并设置属性值。
            /// </summary>
            /// <param name="type"></param>
            /// <param name="propertyName"></param>
            /// <param name="propertyValue"></param>
            public CustPropertyInfo(Type type, string propertyName, object propertyValue) : this(type, propertyName)
            {
                PropertyValue = propertyValue;
            }

            /// <summary>  
            /// 获取或设置属性类型名称。  
            /// </summary>  
            public Type Type { get; set; }

            /// <summary>  
            /// 获取或设置属性名称。  
            /// </summary>  
            public string PropertyName { get; set; }

            /// <summary>
            /// 属性值
            /// </summary>
            public object PropertyValue { get; set; }

            /// <summary>  
            /// 获取属性字段名称。  
            /// </summary>  
            public string FieldName
            {
                get
                {
                    if (PropertyName.Length < 1)
                    {
                        return "";
                    }

                    return PropertyName.Substring(0, 1).ToLower() + PropertyName.Substring(1);
                }
            }

            /// <summary>  
            /// 获取属性在IL中的Set方法名。  
            /// </summary>  
            public string SetPropertyMethodName => "set_" + PropertyName;


            /// <summary>  
            ///  获取属性在IL中的Get方法名。  
            /// </summary>  
            public string GetPropertyMethodName => "get_" + PropertyName;
        }

        #endregion
    }
}