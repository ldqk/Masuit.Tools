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
    /// 反射辅助类
    /// </summary>
    public static class ReflectHelper
    {
        #region 成员读写
        /// <summary>
        /// 获取实体相关属性的值
        /// </summary>
        /// <param name="obj">反射对象</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性值</returns>
        public static object GetInstanceValue(this object obj, string propertyName)
        {
            object objRet = null;
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(obj).Find(propertyName, true);
                if (descriptor != null)
                {
                    objRet = descriptor.GetValue(obj);
                }
            }
            return objRet;
        }
        #endregion

        #region 方法调用
        /// <summary>
        /// 直接调用内部对象的方法/函数或获取属性(支持重载调用)
        /// </summary>
        /// <typeparam name="T">泛型T</typeparam>
        /// <param name="refType">目标数据类型</param>
        /// <param name="funName">函数名称，区分大小写。</param>
        /// <param name="objInitial">如果调用属性，则为相关对象的初始化数据，否则为Null。</param>
        /// <param name="funParams">函数参数信息</param>
        /// <returns>运行结果</returns>
        /// <exception cref="InvalidProgramException">非法异常</exception>
        public static T InvokeMethodOrGetProperty<T>(this Type refType, string funName, object[] objInitial, params object[] funParams)
        {
            MemberInfo[] mis = refType.GetMember(funName);
            if (mis.Length < 1)
            {
                throw new InvalidProgramException(string.Concat("函数/方法 [", funName, "] 在指定类型(", refType.ToString(), ")中不存在！"));
            }
            else
            {
                MethodInfo targetMethod = null;
                StringBuilder pb = new StringBuilder();
                foreach (MemberInfo mi in mis)
                {
                    if (mi.MemberType != MemberTypes.Method)
                    {
                        if (mi.MemberType == MemberTypes.Property)
                        {
                            #region 调用属性方法Get
                            targetMethod = ((PropertyInfo)mi).GetGetMethod();
                            break;
                            #endregion
                        }
                        else
                        {
                            throw new InvalidProgramException(string.Concat("[", funName, "] 不是有效的函数/属性方法！"));
                        }
                    }
                    else
                    {
                        #region 检查函数参数和数据类型 绑定正确的函数到目标调用
                        bool validParamsLen = false, validParamsType = false;

                        MethodInfo curMethod = (MethodInfo)mi;
                        ParameterInfo[] pis = curMethod.GetParameters();
                        if (pis.Length == funParams.Length)
                        {
                            validParamsLen = true;

                            pb = new StringBuilder();
                            bool paramFlag = true;
                            int paramIdx = 0;

                            #region 检查数据类型 设置validParamsType是否有效
                            foreach (ParameterInfo pi in pis)
                            {
                                pb.AppendFormat("Parameter {0}: Type={1}, Name={2}\n", paramIdx, pi.ParameterType, pi.Name);

                                //不对Null和接受Object类型的参数检查
                                if (funParams[paramIdx] != null && pi.ParameterType != typeof(object) && (pi.ParameterType != funParams[paramIdx].GetType()))
                                {
                                    #region 检查类型是否兼容
                                    try
                                    {
                                        funParams[paramIdx] = Convert.ChangeType(funParams[paramIdx], pi.ParameterType);
                                    }
                                    catch (Exception)
                                    {
                                        paramFlag = false;
                                    }
                                    #endregion
                                    //break;
                                }
                                ++paramIdx;
                            }
                            #endregion

                            if (paramFlag)
                            {
                                validParamsType = true;
                            }
                            else
                            {
                                continue;
                            }

                            if (validParamsLen && validParamsType)
                            {
                                targetMethod = curMethod;
                                break;
                            }
                        }
                        #endregion
                    }
                }

                if (targetMethod != null)
                {
                    object objReturn = null;
                    #region 兼顾效率和兼容重载函数调用
                    try
                    {
                        object objInstance = System.Activator.CreateInstance(refType, objInitial);
                        objReturn = targetMethod.Invoke(objInstance, BindingFlags.InvokeMethod, Type.DefaultBinder, funParams,
                            System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        objReturn = refType.InvokeMember(funName, BindingFlags.InvokeMethod, Type.DefaultBinder, null, funParams);
                    }
                    #endregion
                    return (T)objReturn;
                }
                else
                {
                    throw new InvalidProgramException(string.Concat("函数/方法 [", refType.ToString(), ".", funName, "(args ...) ] 参数长度和数据类型不正确！\n 引用参数信息参考：\n", pb.ToString()));
                }
            }
        }

        /// <summary>
        /// 调用相关实体类型的函数方法
        /// </summary>
        /// <typeparam name="T">泛型T</typeparam>
        /// <param name="refType">实体类型</param>
        /// <param name="funName">函数名称</param>
        /// <param name="funParams">函数参数列表</param>
        /// <returns>调用该函数之后的结果</returns>
        public static T InvokeFunction<T>(Type refType, string funName, params object[] funParams)
        {
            return InvokeMethodOrGetProperty<T>(refType, funName, null, funParams);
        }
        #endregion

        #region 资源获取
        /// <summary>
        /// 获取程序集资源的位图资源
        /// </summary>
        /// <param name="assemblyType">程序集中的某一对象类型</param>
        /// <param name="resourceHolder">资源的根名称。例如，名为“MyResource.en-US.resources”的资源文件的根名称为“MyResource”。</param>
        /// <param name="imageName">资源项名称</param>
        public static Bitmap LoadBitmap(Type assemblyType, string resourceHolder, string imageName)
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
        public static string GetStringRes(Type assemblyType, string resName, string resourceHolder)
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
        /// <param name="ResName">嵌入资源相对路径</param>
        /// <returns>如没找到该资源则返回空字符</returns>
        public static string GetManifestString(Type assemblyType, string charset, string ResName)
        {
            Assembly asm = Assembly.GetAssembly(assemblyType);
            Stream st = asm.GetManifestResourceStream(string.Concat(assemblyType.Namespace,
                ".", ResName.Replace("/", ".")));
            if (st == null) { return ""; }
            int iLen = (int)st.Length;
            byte[] bytes = new byte[iLen];
            st.Read(bytes, 0, iLen);
            return (bytes != null) ? Encoding.GetEncoding(charset).GetString(bytes) : "";
        }
        #endregion

    }
}
