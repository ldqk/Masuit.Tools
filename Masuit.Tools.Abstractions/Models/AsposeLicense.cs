using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

namespace Masuit.Tools.Models
{
    /// <summary>
    /// Aspose内存补丁
    /// </summary>
    public static class AsposeLicense
    {
        private static readonly string AsposeList = "Aspose.3D.dll, Aspose.BarCode.dll, Aspose.BarCode.Compact.dll, Aspose.BarCode.WPF.dll, Aspose.Cells.GridDesktop.dll, Aspose.Cells.GridWeb.dll, Aspose.CAD.dll, Aspose.Cells.dll, Aspose.Diagram.dll, Aspose.Email.dll, Aspose.Imaging.dll, Aspose.Note.dll, Aspose.OCR.dll, Aspose.Pdf.dll, Aspose.Slides.dll, Aspose.Tasks.dll, Aspose.Words.dll";

        /// <summary>
        /// 启动Aspose的内存破解
        /// </summary>
        public static void ActivateMemoryPatching()
        {
            Assembly[] arr = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in arr)
            {
                if (AsposeList.IndexOf(assembly.FullName.Split(',')[0] + ".dll") != -1)
                    ActivateForAssembly(assembly);
            }
            AppDomain.CurrentDomain.AssemblyLoad += ActivateOnLoad;
        }

        private static void ActivateOnLoad(object sender, AssemblyLoadEventArgs e)
        {
            if (AsposeList.IndexOf(e.LoadedAssembly.FullName.Split(',')[0] + ".dll") != -1)
                ActivateForAssembly(e.LoadedAssembly);
        }

        private static void ActivateForAssembly(Assembly assembly)
        {
            MethodInfo miLicensed1 = typeof(AsposeLicense).GetMethod("InvokeMe1", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo miLicensed2 = typeof(AsposeLicense).GetMethod("InvokeMe2", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo miEvaluation = null;

            Dictionary<string, MethodInfo> miDict = new Dictionary<string, MethodInfo>
            {
                ["System.DateTime"] = miLicensed1,
                ["System.Xml.XmlElement"] = miLicensed2
            };

            Type[] arrType = null;
            bool isFound = false;
            int nCount = 0;

            try
            {
                arrType = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException err)
            {
                arrType = err.Types;
            }
            foreach (Type type in arrType)
            {
                if (isFound) break;
                if (type == null) continue;
                MethodInfo[] arrMInfo = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
                foreach (MethodInfo info in arrMInfo)
                {
                    if (isFound) break;
                    try
                    {
                        string strMethod = info.ToString();
                        if ((strMethod.IndexOf("(System.Xml.XmlElement, System.String)") > 0) && miDict.ContainsKey(info.ReturnType.ToString()))
                        {
                            miEvaluation = info;
                            MemoryPatching(miEvaluation, miDict[miEvaluation.ReturnType.ToString()]);
                            nCount++;
                            if (((assembly.FullName.IndexOf("Aspose.Pdf") == -1) && (nCount == 2)) || ((assembly.FullName.IndexOf("Aspose.Pdf") != -1) && (nCount == 6)))
                            {
                                isFound = true;
                                break;
                            }
                        }
                    }
                    catch
                    {
                        throw new InvalidOperationException("MemoryPatching for \"" + assembly.FullName + "\" failed !");
                    }
                }
            }

            string[] aParts = assembly.FullName.Split(',');
            string fName = aParts[0];
            if (fName.IndexOf("Aspose.BarCode.") != -1)
                fName = "Aspose.BarCode";
            else if (fName.IndexOf("Aspose.3D") != -1)
                fName = "Aspose.ThreeD";

            try
            {
                Type type2 = assembly.GetType(fName + ".License");
                MethodInfo mi = type2.GetMethod("SetLicense", new[] { typeof(Stream) });
                const string LData = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4KPExpY2Vuc2U+CiAgPERhdGE+CiAgICA8TGljZW5zZWRUbz5MaWNlbnNlZTwvTGljZW5zZWRUbz4KICAgIDxFbWFpbFRvPmxpY2Vuc2VlQGVtYWlsLmNvbTwvRW1haWxUbz4KICAgIDxMaWNlbnNlVHlwZT5EZXZlbG9wZXIgT0VNPC9MaWNlbnNlVHlwZT4KICAgIDxMaWNlbnNlTm90ZT5MaW1pdGVkIHRvIDEwMDAgZGV2ZWxvcGVyLCB1bmxpbWl0ZWQgcGh5c2ljYWwgbG9jYXRpb25zPC9MaWNlbnNlTm90ZT4KICAgIDxPcmRlcklEPjc4NDM3ODU3Nzg1PC9PcmRlcklEPgogICAgPFVzZXJJRD4xMTk3ODkyNDM3OTwvVXNlcklEPgogICAgPE9FTT5UaGlzIGlzIGEgcmVkaXN0cmlidXRhYmxlIGxpY2Vuc2U8L09FTT4KICAgIDxQcm9kdWN0cz4KICAgICAgPFByb2R1Y3Q+QXNwb3NlLlRvdGFsIFByb2R1Y3QgRmFtaWx5PC9Qcm9kdWN0PgogICAgPC9Qcm9kdWN0cz4KICAgIDxFZGl0aW9uVHlwZT5FbnRlcnByaXNlPC9FZGl0aW9uVHlwZT4KICAgIDxTZXJpYWxOdW1iZXI+e0YyQjk3MDQ1LTFCMjktNEIzRi1CRDUzLTYwMUVGRkExNUFBOX08L1NlcmlhbE51bWJlcj4KICAgIDxTdWJzY3JpcHRpb25FeHBpcnk+MjA5OTEyMzE8L1N1YnNjcmlwdGlvbkV4cGlyeT4KICAgIDxMaWNlbnNlVmVyc2lvbj4zLjA8L0xpY2Vuc2VWZXJzaW9uPgogIDwvRGF0YT4KICA8U2lnbmF0dXJlPlFYTndiM05sTGxSdmRHRnNJRkJ5YjJSMVkzUWdSbUZ0YVd4NTwvU2lnbmF0dXJlPgo8L0xpY2Vuc2U+";
                Stream stream = new MemoryStream(Convert.FromBase64String(LData));
                stream.Seek(0, SeekOrigin.Begin);
                mi.Invoke(Activator.CreateInstance(type2, null), new[] { stream });
            }
            catch
            {
                throw new InvalidOperationException("SetLicense for \"" + assembly.FullName + "\" failed !");
            }
        }

        private static DateTime InvokeMe1(XmlElement element, string name)
        {
            return DateTime.MaxValue;
        }

        private static XmlElement InvokeMe2(XmlElement element, string name)
        {
            if (element.LocalName == "License")
            {
                const string License64 = "PERhdGE+PExpY2Vuc2VkVG8+R3JvdXBEb2NzPC9MaWNlbnNlZFRvPjxMaWNlbnNlVHlwZT5TaXRlIE9FTTwvTGljZW5zZVR5cGU+PExpY2Vuc2VOb3RlPkxpbWl0ZWQgdG8gMTAgZGV2ZWxvcGVyczwvTGljZW5zZU5vdGU+PE9yZGVySUQ+MTMwNzI0MDQwODQ5PC9PcmRlcklEPjxPRU0+VGhpcyBpcyBhIHJlZGlzdHJpYnV0YWJsZSBsaWNlbnNlPC9PRU0+PFByb2R1Y3RzPjxQcm9kdWN0PkFzcG9zZS5Ub3RhbDwvUHJvZHVjdD48L1Byb2R1Y3RzPjxFZGl0aW9uVHlwZT5FbnRlcnByaXNlPC9FZGl0aW9uVHlwZT48U2VyaWFsTnVtYmVyPjliNTc5NTAxLTUyNjEtNDIyMC04NjcwLWZjMmQ4Y2NkZDkwYzwvU2VyaWFsTnVtYmVyPjxTdWJzY3JpcHRpb25FeHBpcnk+MjAxNDA3MjQ8L1N1YnNjcmlwdGlvbkV4cGlyeT48TGljZW5zZVZlcnNpb24+Mi4yPC9MaWNlbnNlVmVyc2lvbj48L0RhdGE+PFNpZ25hdHVyZT5udFpocmRoL3I0QS81ZFpsU2dWYnhac0hYSFBxSjZ5UVVYa0RvaW4vS2lVZWhUUWZET0lQdHdzUlR2NmRTUVplOVdXekNnV3RGdkdROWpmR2QySmF4YUQvbkx1ZEk2R0VVajhqeVhUMG4vbWRrMEF1WVZNYlBXRjJYd3dSTnFlTmRrblYyQjhrZVFwbDJ2RzZVbnhxS2J6VVFxS2Rhc1pzZ2w1Q0xqSFVEWms9PC9TaWduYXR1cmU+";
                element.InnerXml = new UTF8Encoding().GetString(Convert.FromBase64String(License64));
            }

            if (element.LocalName == "BlackList")
            {
                const string BlackList64 = "PERhdGE+PC9EYXRhPjxTaWduYXR1cmU+cUJwMEx1cEVoM1ZnOWJjeS8vbUVXUk9KRWZmczRlY25iTHQxYlNhanU2NjY5RHlad09FakJ1eEdBdVBxS1hyd0x5bmZ5VWplYUNGQ0QxSkh2RVUxVUl5eXJOTnBSMXc2NXJIOUFyUCtFbE1lVCtIQkZ4NFMzckFVMnd6dkxPZnhGeU9DQ0dGQ2UraTdiSHlGQk44WHp6R1UwdGRPMGR1RTFoRTQ5M1RNY3pRPTwvU2lnbmF0dXJlPg==";
                element.InnerXml = new UTF8Encoding().GetString(Convert.FromBase64String(BlackList64));
            }

            XmlNodeList elementsByTagName = element.GetElementsByTagName(name);
            if (elementsByTagName.Count <= 0)
                return null;

            return (XmlElement)elementsByTagName[0];
        }

        private static unsafe void MemoryPatching(MethodBase miEvaluation, MethodBase miLicensed)
        {
            IntPtr IntPtrEval = GetMemoryAddress(miEvaluation);
            IntPtr IntPtrLicensed = GetMemoryAddress(miLicensed);

            if (IntPtr.Size == 8)
                *(long*)IntPtrEval.ToPointer() = *(long*)IntPtrLicensed.ToPointer();
            else
                *(int*)IntPtrEval.ToPointer() = *(int*)IntPtrLicensed.ToPointer();
        }

        private static unsafe IntPtr GetMemoryAddress(MethodBase mb)
        {
            RuntimeHelpers.PrepareMethod(mb.MethodHandle);

            if ((Environment.Version.Major >= 4) || ((Environment.Version.Major == 2) && (Environment.Version.MinorRevision >= 3053)))
                return new IntPtr((int*)mb.MethodHandle.Value.ToPointer() + 2);

            ulong* location = (ulong*)mb.MethodHandle.Value.ToPointer();
            int index = (int)((*location >> 32) & 0xFF);
            if (IntPtr.Size == 8)
            {
                ulong* classStart = (ulong*)mb.DeclaringType.TypeHandle.Value.ToPointer();
                ulong* address = classStart + index + 10;
                return new IntPtr(address);
            }
            else
            {
                uint* classStart = (uint*)mb.DeclaringType.TypeHandle.Value.ToPointer();
                uint* address = classStart + index + 10;
                return new IntPtr(address);
            }
        }
    }
}

//End