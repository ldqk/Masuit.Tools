using Masuit.Tools.Strings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Masuit.Tools
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class Extensions
    {
        #region SyncForEach

        /// <summary>
        /// 遍历数组
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="action">回调方法</param>
        public static void ForEach(this object[] objs, Action<object> action)
        {
            foreach (var o in objs)
            {
                action(o);
            }
        }

        /// <summary>
        /// 遍历IEnumerable
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="action">回调方法</param>
        public static void ForEach(this IEnumerable<dynamic> objs, Action<object> action)
        {
            foreach (var o in objs)
            {
                action(o);
            }
        }

        /// <summary>
        /// 遍历集合
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="action">回调方法</param>
        public static void ForEach(this IList<dynamic> objs, Action<object> action)
        {
            foreach (var o in objs)
            {
                action(o);
            }
        }

        /// <summary>
        /// 遍历数组
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="action">回调方法</param>
        /// <typeparam name="T"></typeparam>
        public static void ForEach<T>(this T[] objs, Action<T> action)
        {
            foreach (var o in objs)
            {
                action(o);
            }
        }

        /// <summary>
        /// 遍历IEnumerable
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="action">回调方法</param>
        /// <typeparam name="T"></typeparam>
        public static void ForEach<T>(this IEnumerable<T> objs, Action<T> action)
        {
            foreach (var o in objs)
            {
                action(o);
            }
        }

        /// <summary>
        /// 遍历List
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="action">回调方法</param>
        /// <typeparam name="T"></typeparam>
        public static void ForEach<T>(this IList<T> objs, Action<T> action)
        {
            foreach (var o in objs)
            {
                action(o);
            }
        }

        /// <summary>
        /// 遍历数组并返回一个新的List
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="action">回调方法</param>
        /// <returns></returns>
        public static IEnumerable<T> ForEach<T>(this object[] objs, Func<object, T> action)
        {
            foreach (var o in objs)
            {
                yield return action(o);
            }
        }

        /// <summary>
        /// 遍历IEnumerable并返回一个新的List
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="action">回调方法</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<dynamic> objs, Func<object, T> action)
        {
            foreach (var o in objs)
            {
                yield return action(o);
            }
        }

        /// <summary>
        /// 遍历List并返回一个新的List
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="action">回调方法</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> ForEach<T>(this IList<dynamic> objs, Func<object, T> action)
        {
            foreach (var o in objs)
            {
                yield return action(o);
            }
        }


        /// <summary>
        /// 遍历数组并返回一个新的List
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="action">回调方法</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> ForEach<T>(this T[] objs, Func<T, T> action)
        {
            foreach (var o in objs)
            {
                yield return action(o);
            }
        }

        /// <summary>
        /// 遍历IEnumerable并返回一个新的List
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="action">回调方法</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> objs, Func<T, T> action)
        {
            foreach (var o in objs)
            {
                yield return action(o);
            }
        }

        /// <summary>
        /// 遍历List并返回一个新的List
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="action">回调方法</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> ForEach<T>(this IList<T> objs, Func<T, T> action)
        {
            foreach (var o in objs)
            {
                yield return action(o);
            }
        }

        #endregion

        #region AsyncForEach

        /// <summary>
        /// 遍历数组
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="action">回调方法</param>
        public static async void ForEachAsync(this object[] objs, Action<object> action)
        {
            await Task.Run(() =>
            {
                Parallel.ForEach(objs, action);
            });
        }

        /// <summary>
        /// 遍历数组
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="action">回调方法</param>
        /// <typeparam name="T"></typeparam>
        public static async void ForEachAsync<T>(this T[] objs, Action<T> action)
        {
            await Task.Run(() =>
            {
                Parallel.ForEach(objs, action);
            });
        }

        /// <summary>
        /// 遍历IEnumerable
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="action">回调方法</param>
        /// <typeparam name="T"></typeparam>
        public static async void ForEachAsync<T>(this IEnumerable<T> objs, Action<T> action)
        {
            await Task.Run(() =>
            {
                Parallel.ForEach(objs, action);
            });
        }

        /// <summary>
        /// 遍历List
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="action">回调方法</param>
        /// <typeparam name="T"></typeparam>
        public static async void ForEachAsync<T>(this IList<T> objs, Action<T> action)
        {
            await Task.Run(() =>
            {
                Parallel.ForEach(objs, action);
            });
        }

        #endregion

        #region Map

        /// <summary>
        /// 映射到目标类型(浅克隆)
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型</returns>
        public static TDestination MapTo<TDestination>(this object source) where TDestination : new()
        {
            TDestination dest = new TDestination();
            dest.GetType().GetProperties().ForEach(p =>
            {
                p.SetValue(dest, source.GetType().GetProperty(p.Name)?.GetValue(source));
            });
            return dest;
        }

        /// <summary>
        /// 映射到目标类型(浅克隆)
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型</returns>
        public static async Task<TDestination> MapToAsync<TDestination>(this object source) where TDestination : new()
        {
            return await Task.Run(() =>
            {
                TDestination dest = new TDestination();
                dest.GetType().GetProperties().ForEach(p =>
                {
                    p.SetValue(dest, source.GetType().GetProperty(p.Name)?.GetValue(source));
                });
                return dest;
            });
        }

        /// <summary>
        /// 映射到目标类型(深克隆)
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型</returns>
        public static TDestination Map<TDestination>(this object source) where TDestination : new() => JsonConvert.DeserializeObject<TDestination>(JsonConvert.SerializeObject(source));

        /// <summary>
        /// 映射到目标类型(深克隆)
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型</returns>
        public static async Task<TDestination> MapAsync<TDestination>(this object source) where TDestination : new() => await Task.Run(() => JsonConvert.DeserializeObject<TDestination>(JsonConvert.SerializeObject(source)));

        /// <summary>
        /// 复制一个新的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Copy<T>(this T source) where T : new()
        {
            T dest = new T();
            dest.GetType().GetProperties().ForEach(p =>
            {
                p.SetValue(dest, source.GetType().GetProperty(p.Name)?.GetValue(source));
            });
            return dest;
        }

        /// <summary>
        /// 复制到一个现有对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">源对象</param>
        /// <param name="dest">目标对象</param>
        /// <returns></returns>
        public static T CopyTo<T>(this T source, T dest) where T : new()
        {
            dest.GetType().GetProperties().ForEach(p =>
            {
                p.SetValue(dest, source.GetType().GetProperty(p.Name)?.GetValue(source));
            });
            return dest;
        }

        /// <summary>
        /// 复制一个新的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static async Task<T> CopyAsync<T>(this T source) where T : new() => await Task.Run(() =>
        {
            T dest = new T();
            dest.GetType().GetProperties().ForEach(p =>
            {
                p.SetValue(dest, source.GetType().GetProperty(p.Name)?.GetValue(source));
            });
            return dest;
        });

        /// <summary>
        /// 映射到目标类型的集合
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型集合</returns>
        public static IEnumerable<TDestination> ToList<TDestination>(this object[] source) where TDestination : new()
        {
            foreach (var o in source)
            {
                var dest = new TDestination();
                dest.GetType().GetProperties().ForEach(p =>
                {
                    p.SetValue(dest, source.GetType().GetProperty(p.Name)?.GetValue(o));
                });
                yield return dest;
            }
        }

        /// <summary>
        /// 映射到目标类型的集合
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型集合</returns>
        public static async Task<IEnumerable<TDestination>> ToListAsync<TDestination>(this object[] source) where TDestination : new()
        {
            return await Task.Run(() =>
            {
                IList<TDestination> list = new List<TDestination>();
                foreach (var o in source)
                {
                    var dest = new TDestination();
                    dest.GetType().GetProperties().ForEach(p =>
                    {
                        p.SetValue(dest, source.GetType().GetProperty(p.Name)?.GetValue(o));
                    });
                    list.Add(dest);
                }

                return list;
            });
        }

        /// <summary>
        /// 映射到目标类型的集合
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型集合</returns>
        public static IEnumerable<TDestination> ToList<TDestination>(this IList<dynamic> source) where TDestination : new()
        {
            foreach (var o in source)
            {
                var dest = new TDestination();
                dest.GetType().GetProperties().ForEach(p =>
                {
                    p.SetValue(dest, source.GetType().GetProperty(p.Name)?.GetValue(o));
                });
                yield return dest;
            }
        }

        /// <summary>
        /// 映射到目标类型的集合
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型集合</returns>
        public static async Task<IEnumerable<TDestination>> ToListAsync<TDestination>(this IList<dynamic> source) where TDestination : new()
        {
            return await Task.Run(() =>
            {
                IList<TDestination> list = new List<TDestination>();
                foreach (var o in source)
                {
                    var dest = new TDestination();
                    dest.GetType().GetProperties().ForEach(p =>
                    {
                        p.SetValue(dest, source.GetType().GetProperty(p.Name)?.GetValue(o));
                    });
                    list.Add(dest);
                }

                return list;
            });
        }

        /// <summary>
        /// 映射到目标类型的集合
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型集合</returns>
        public static IEnumerable<TDestination> ToList<TDestination>(this IEnumerable<dynamic> source) where TDestination : new()
        {
            foreach (var o in source)
            {
                var dest = new TDestination();
                dest.GetType().GetProperties().ForEach(p =>
                {
                    p.SetValue(dest, source.GetType().GetProperty(p.Name)?.GetValue(o));
                });
                yield return dest;
            }
        }

        /// <summary>
        /// 映射到目标类型的集合
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型集合</returns>
        public static async Task<IEnumerable<TDestination>> ToListAsync<TDestination>(this IEnumerable<dynamic> source) where TDestination : new()
        {
            return await Task.Run(() =>
            {
                IList<TDestination> list = new List<TDestination>();
                foreach (var o in source)
                {
                    var dest = new TDestination();
                    dest.GetType().GetProperties().ForEach(p =>
                    {
                        p.SetValue(dest, source.GetType().GetProperty(p.Name)?.GetValue(o));
                    });
                    list.Add(dest);
                }

                return list;
            });
        }

        #endregion

        /// <summary>
        /// 转换成json字符串
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToJsonString(this object source) => JsonConvert.SerializeObject(source, new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        /// <summary>
        /// 转换成json字符串
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static async Task<string> ToJsonStringAsync(this object source) => await Task.Run(() => JsonConvert.SerializeObject(source));

        #region UBB、HTML互转

        /// <summary>
        /// UBB代码处理函数
        /// </summary>
        /// <param name="ubbStr">输入UBB字符串</param>
        /// <returns>输出html字符串</returns>
        public static string UbbToHtml(this string ubbStr)
        {
            Regex r;
            Match m;

            #region 处理空格

            ubbStr = ubbStr.Replace(" ", "&nbsp;");

            #endregion

            #region 处理&符

            ubbStr = ubbStr.Replace("&", "&amp;");

            #endregion

            #region 处理单引号

            ubbStr = ubbStr.Replace("'", "’");

            #endregion

            #region 处理双引号

            ubbStr = ubbStr.Replace("\"", "&quot;");

            #endregion

            #region html标记符

            ubbStr = ubbStr.Replace("<", "&lt;");
            ubbStr = ubbStr.Replace(">", "&gt;");

            #endregion

            #region 处理换行

            //处理换行，在每个新行的前面添加两个全角空格
            r = new Regex(@"(\r\n((&nbsp;)|　)+)(?<正文>\S+)", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch()) ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<BR>　　" + m.Groups["正文"]);
            //处理换行，在每个新行的前面添加两个全角空格
            ubbStr = ubbStr.Replace("\r\n", "<BR>");

            #endregion

            #region 处[b][/b]标记

            r = new Regex(@"(\[b\])([ \S\t]*?)(\[\/b\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch()) ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<B>" + m.Groups[2] + "</B>");

            #endregion

            #region 处[i][/i]标记

            r = new Regex(@"(\[i\])([ \S\t]*?)(\[\/i\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch()) ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<I>" + m.Groups[2] + "</I>");

            #endregion

            #region 处[u][/u]标记

            r = new Regex(@"(\[U\])([ \S\t]*?)(\[\/U\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch()) ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<U>" + m.Groups[2] + "</U>");

            #endregion

            #region 处[p][/p]标记

            //处[p][/p]标记
            r = new Regex(@"((\r\n)*\[p\])(.*?)((\r\n)*\[\/p\])", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch()) ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<P class=\"pstyle\">" + m.Groups[3] + "</P>");

            #endregion

            #region 处[sup][/sup]标记

            //处[sup][/sup]标记
            r = new Regex(@"(\[sup\])([ \S\t]*?)(\[\/sup\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch()) ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<SUP>" + m.Groups[2] + "</SUP>");

            #endregion

            #region 处[sub][/sub]标记

            //处[sub][/sub]标记
            r = new Regex(@"(\[sub\])([ \S\t]*?)(\[\/sub\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch()) ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<SUB>" + m.Groups[2] + "</SUB>");

            #endregion

            #region 处标记

            //处标记
            r = new Regex(@"(\[url\])([ \S\t]*?)(\[\/url\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<A href=\"" + m.Groups[2] + "\" target=\"_blank\">" + m.Groups[2] + "</A>");
            }

            #endregion

            #region 处[url=xxx][/url]标记

            //处[url=xxx][/url]标记
            r = new Regex(@"(\[url=([ \S\t]+)\])([ \S\t]*?)(\[\/url\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<A href=\"" + m.Groups[2] + "\" target=\"_blank\">" + m.Groups[3] + "</A>");
            }

            #endregion

            #region 处[email][/email]标记

            //处[email][/email]标记
            r = new Regex(@"(\[email\])([ \S\t]*?)(\[\/email\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<A href=\"mailto:" + m.Groups[2] + "\" target=\"_blank\">" + m.Groups[2] + "</A>");
            }

            #endregion

            #region 处[email=xxx][/email]标记

            //处[email=xxx][/email]标记
            r = new Regex(@"(\[email=([ \S\t]+)\])([ \S\t]*?)(\[\/email\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<A href=\"mailto:" + m.Groups[2] + "\" target=\"_blank\">" + m.Groups[3] + "</A>");
            }

            #endregion

            #region 处[size=x][/size]标记

            //处[size=x][/size]标记
            r = new Regex(@"(\[size=([1-7])\])([ \S\t]*?)(\[\/size\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<FONT SIZE=" + m.Groups[2] + ">" + m.Groups[3] + "</FONT>");
            }

            #endregion

            #region 处[color=x][/color]标记

            //处[color=x][/color]标记
            r = new Regex(@"(\[color=([\S]+)\])([ \S\t]*?)(\[\/color\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<FONT COLOR=" + m.Groups[2] + ">" + m.Groups[3] + "</FONT>");
            }

            #endregion

            #region 处[font=x][/font]标记

            //处[font=x][/font]标记
            r = new Regex(@"(\[font=([\S]+)\])([ \S\t]*?)(\[\/font\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<FONT FACE=" + m.Groups[2] + ">" + m.Groups[3] + "</FONT>");
            }

            #endregion

            #region 处理图片链接

            //处理图片链接
            r = new Regex("\\[picture\\](\\d+?)\\[\\/picture\\]", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<A href=\"ShowImage.aspx?Type=ALL&Action=forumImage&ImageID=" + m.Groups[1] + "\" target=\"_blank\"><IMG border=0 Title=\"点击打开新窗口查看\" src=\"ShowImage.aspx?Action=forumImage&ImageID=" + m.Groups[1] + "\"></A>");
            }

            #endregion

            #region 处理[align=x][/align]

            //处理[align=x][/align]
            r = new Regex(@"(\[align=([\S]+)\])([ \S\t]*?)(\[\/align\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<P align=" + m.Groups[2] + ">" + m.Groups[3] + "</P>");
            }

            #endregion

            #region 处[H=x][/H]标记

            //处[H=x][/H]标记
            r = new Regex(@"(\[H=([1-6])\])([ \S\t]*?)(\[\/H\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<H" + m.Groups[2] + ">" + m.Groups[3] + "</H" + m.Groups[2] + ">");
            }

            #endregion

            #region 处理[list=x][*][/list]

            //处理[list=x][*][/list]
            r = new Regex(@"(\[list(=(A|a|I|i| ))?\]([ \S\t]*)\r\n)((\[\*\]([ \S\t]*\r\n))*?)(\[\/list\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                string strLi = m.Groups[5].ToString();
                Regex rLi = new Regex(@"\[\*\]([ \S\t]*\r\n?)", RegexOptions.IgnoreCase);
                Match mLi;
                for (mLi = rLi.Match(strLi); mLi.Success; mLi = mLi.NextMatch()) strLi = strLi.Replace(mLi.Groups[0].ToString(), "<LI>" + mLi.Groups[1]);
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<UL TYPE=\"" + m.Groups[3] + "\"><B>" + m.Groups[4] + "</B>" + strLi + "</UL>");
            }

            #endregion

            #region 处[SHADOW=x][/SHADOW]标记

            //处[SHADOW=x][/SHADOW]标记
            r = new Regex(@"(\[SHADOW=)(\d*?),(#*\w*?),(\d*?)\]([\S\t]*?)(\[\/SHADOW\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<TABLE WIDTH=" + m.Groups[2] + "STYLE=FILTER:SHADOW(COLOR=" + m.Groups[3] + ",STRENGTH=" + m.Groups[4] + ")>" + m.Groups[5] + "</TABLE>");
            }

            #endregion

            #region 处[glow=x][/glow]标记

            //处[glow=x][/glow]标记
            r = new Regex(@"(\[glow=)(\d*?),(#*\w*?),(\d*?)\]([\S\t]*?)(\[\/glow\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<TABLE WIDTH=" + m.Groups[2] + "  STYLE=FILTER:GLOW(COLOR=" + m.Groups[3] + ", STRENGTH=" + m.Groups[4] + ")>" + m.Groups[5] + "</TABLE>");
            }

            #endregion

            #region 处[center][/center]标记

            r = new Regex(@"(\[center\])([ \S\t]*?)(\[\/center\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch()) ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<CENTER>" + m.Groups[2] + "</CENTER>");

            #endregion

            #region 处[ IMG][ /IMG]标记

            r = new Regex(@"(\[IMG\])(http|https|ftp):\/\/([ \S\t]*?)(\[\/IMG\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch()) ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<br><a onfocus=this.blur() href=" + m.Groups[2] + "://" + m.Groups[3] + " target=_blank><IMG SRC=" + m.Groups[2] + "://" + m.Groups[3] + " border=0 alt=按此在新窗口浏览图片 onload=javascript:if(screen.width-333<this.width)this.width=screen.width-333></a>");

            #endregion

            #region 处[em]标记

            r = new Regex(@"(\[em([\S\t]*?)\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch()) ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<img src=pic/em" + m.Groups[2] + ".gif border=0 align=middle>");

            #endregion

            #region 处[flash=x][/flash]标记

            //处[mp=x][/mp]标记
            r = new Regex(@"(\[flash=)(\d*?),(\d*?)\]([\S\t]*?)(\[\/flash\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<a href=" + m.Groups[4] + " TARGET=_blank><IMG SRC=pic/swf.gif border=0 alt=点击开新窗口欣赏该FLASH动画!> [全屏欣赏]</a><br><br><OBJECT codeBase=http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=6,0,29,0 classid=clsid:D27CDB6E-AE6D-11cf-96B8-444553540000 width=" + m.Groups[2] + " height=" + m.Groups[3] + "><PARAM NAME=movie VALUE=" + m.Groups[4] + "><PARAM NAME=quality VALUE=high><param name=menu value=false><embed src=" + m.Groups[4] + " quality=high menu=false pluginspage=http://www.macromedia.com/go/getflashplayer type=application/x-shockwave-flash width=" + m.Groups[2] + " height=" + m.Groups[3] + ">" + m.Groups[4] + "</embed></OBJECT>");
            }

            #endregion

            #region 处[dir=x][/dir]标记

            //处[dir=x][/dir]标记
            r = new Regex(@"(\[dir=)(\d*?),(\d*?)\]([\S\t]*?)(\[\/dir\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<object classid=clsid:166B1BCA-3F9C-11CF-8075-444553540000 codebase=http://download.macromedia.com/pub/shockwave/cabs/director/sw.cab#version=7,0,2,0 width=" + m.Groups[2] + " height=" + m.Groups[3] + "><param name=src value=" + m.Groups[4] + "><embed src=" + m.Groups[4] + " pluginspage=http://www.macromedia.com/shockwave/download/ width=" + m.Groups[2] + " height=" + m.Groups[3] + "></embed></object>");
            }

            #endregion

            #region 处[rm=x][/rm]标记

            //处[rm=x][/rm]标记
            r = new Regex(@"(\[rm=)(\d*?),(\d*?)\]([\S\t]*?)(\[\/rm\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<OBJECT classid=clsid:CFCDAA03-8BE4-11cf-B84B-0020AFBBCCFA class=OBJECT id=RAOCX width=" + m.Groups[2] + " height=" + m.Groups[3] + "><PARAM NAME=SRC VALUE=" + m.Groups[4] + "><PARAM NAME=CONSOLE VALUE=Clip1><PARAM NAME=CONTROLS VALUE=imagewindow><PARAM NAME=AUTOSTART VALUE=true></OBJECT><br><OBJECT classid=CLSID:CFCDAA03-8BE4-11CF-B84B-0020AFBBCCFA height=32 id=video2 width=" + m.Groups[2] + "><PARAM NAME=SRC VALUE=" + m.Groups[4] + "><PARAM NAME=AUTOSTART VALUE=-1><PARAM NAME=CONTROLS VALUE=controlpanel><PARAM NAME=CONSOLE VALUE=Clip1></OBJECT>");
            }

            #endregion

            #region 处[mp=x][/mp]标记

            //处[mp=x][/mp]标记
            r = new Regex(@"(\[mp=)(\d*?),(\d*?)\]([\S\t]*?)(\[\/mp\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<object align=middle classid=CLSID:22d6f312-b0f6-11d0-94ab-0080c74c7e95 class=OBJECT id=MediaPlayer width=" + m.Groups[2] + " height=" + m.Groups[3] + " ><param name=ShowStatusBar value=-1><param name=Filename value=" + m.Groups[4] + "><embed type=application/x-oleobject codebase=http://activex.microsoft.com/activex/controls/mplayer/en/nsmp2inf.cab#Version=5,1,52,701 flename=mp src=" + m.Groups[4] + "  width=" + m.Groups[2] + " height=" + m.Groups[3] + "></embed></object>");
            }

            #endregion

            #region 处[qt=x][/qt]标记

            //处[qt=x][/qt]标记
            r = new Regex(@"(\[qt=)(\d*?),(\d*?)\]([\S\t]*?)(\[\/qt\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<embed src=" + m.Groups[4] + " width=" + m.Groups[2] + " height=" + m.Groups[3] + " autoplay=true loop=false controller=true playeveryframe=false cache=false scale=TOFIT bgcolor=#000000 kioskmode=false targetcache=false pluginspage=http://www.apple.com/quicktime/>");
            }

            #endregion

            #region 处[QUOTE][/QUOTE]标记

            r = new Regex(@"(\[QUOTE\])([ \S\t]*?)(\[\/QUOTE\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch()) ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<div style='border:#CCCCCC 1px dashed; width:94%; color:#999999; padding:3px; background:#F8F8F8'>" + m.Groups[2] + "</div><br /> ");

            #endregion

            #region 处[move][/move]标记

            r = new Regex(@"(\[move\])([ \S\t]*?)(\[\/move\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch()) ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<MARQUEE scrollamount=3>" + m.Groups[2] + "</MARQUEE>");

            #endregion

            #region 处[FLY][/FLY]标记

            r = new Regex(@"(\[FLY\])([ \S\t]*?)(\[\/FLY\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch()) ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<MARQUEE width=80% behavior=alternate scrollamount=3>" + m.Groups[2] + "</MARQUEE>");

            #endregion

            #region 处[image][/image]标记

            //处[image][/image]标记
            r = new Regex(@"(\[image\])([ \S\t]*?)(\[\/image\])", RegexOptions.IgnoreCase);
            for (m = r.Match(ubbStr); m.Success; m = m.NextMatch())
            {
                ubbStr = ubbStr.Replace(m.Groups[0].ToString(), "<img src=\"" + m.Groups[2] + "\" border=0 align=middle><br>");
            }

            #endregion

            return ubbStr;
        }

        /// <summary>
        /// UBB代码处理函数
        /// </summary>
        /// <param name="ubbStr">输入UBB字符串</param>
        /// <returns>输出html字符串</returns>
        public static async Task<string> UbbToHtmlAsync(this string ubbStr) => await Task.Run(() => UbbToHtml(ubbStr));

        /// <summary>
        /// UBB转HTML方式2
        /// </summary>
        /// <param name="ubbStr">UBB 代码</param>
        /// <returns>HTML代码</returns>
        public static string UbbToHtml2(this string ubbStr)
        {
            ubbStr = ubbStr.Replace("&", "&amp;");
            ubbStr = ubbStr.Replace("<", "&lt;");
            ubbStr = ubbStr.Replace(">", "&gt;");
            ubbStr = ubbStr.Replace(" ", "&nbsp;"); //空格
            ubbStr = ubbStr.Replace("\n", "<br>"); //回车
            Regex my = new Regex(@"(\[IMG\])(.[^\[]*)(\[\/IMG\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<a href=""$2"" target=_blank><IMG SRC=""$2"" border=0 alt=按此在新窗口浏览图片 onload=""javascript:if(this.width>screen.width-333)this.width=screen.width-333""></a>");

            my = new Regex(@"\[DIR=*([0-9]*),*([0-9]*)\](.[^\[]*)\[\/DIR]", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<object classid=clsid:166B1BCA-3F9C-11CF-8075-444553540000 codebase=http://download.macromedia.com/pub/shockwave/cabs/director/sw.cab#version=7,0,2,0 width=$1 height=$2><param name=src value=$3><embed src=$3 pluginspage=http://www.macromedia.com/shockwave/download/ width=$1 height=$2></embed></object>");

            my = new Regex(@"\[QT=*([0-9]*),*([0-9]*)\](.[^\[]*)\[\/QT]", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<embed src=$3 width=$1 height=$2 autoplay=true loop=false controller=true playeveryframe=false cache=false scale=TOFIT bgcolor=#000000 kioskmode=false targetcache=false pluginspage=http://www.apple.com/quicktime/>");

            my = new Regex(@"\[MP=*([0-9]*),*([0-9]*)\](.[^\[]*)\[\/MP]", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<object align=middle classid=CLSID:22d6f312-b0f6-11d0-94ab-0080c74c7e95 class=OBJECT id=MediaPlayer width=$1 height=$2 ><param name=ShowStatusBar value=-1><param name=Filename value=$3><embed type=application/x-oleobject codebase=http://activex.microsoft.com/activex/controls/mplayer/en/nsmp2inf.cab#Version=5,1,52,701 flename=mp src=$3  width=$1 height=$2></embed></object>");

            my = new Regex(@"\[RM=*([0-9]*),*([0-9]*)\](.[^\[]*)\[\/RM]", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<OBJECT classid=clsid:CFCDAA03-8BE4-11cf-B84B-0020AFBBCCFA class=OBJECT id=RAOCX width=$1 height=$2><PARAM NAME=SRC VALUE=$3><PARAM NAME=CONSOLE VALUE=Clip1><PARAM NAME=CONTROLS VALUE=imagewindow><PARAM NAME=AUTOSTART VALUE=true></OBJECT><br><OBJECT classid=CLSID:CFCDAA03-8BE4-11CF-B84B-0020AFBBCCFA height=32 id=video2 width=$1><PARAM NAME=SRC VALUE=$3><PARAM NAME=AUTOSTART VALUE=-1><PARAM NAME=CONTROLS VALUE=controlpanel><PARAM NAME=CONSOLE VALUE=Clip1></OBJECT>");

            my = new Regex(@"(\[FLASH\])(.[^\[]*)(\[\/FLASH\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<OBJECT codeBase=http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=4,0,2,0 classid=clsid:D27CDB6E-AE6D-11cf-96B8-444553540000 width=500 height=400><PARAM NAME=movie VALUE=""$2""><PARAM NAME=quality VALUE=high><embed src=""$2"" quality=high pluginspage='http://www.macromedia.com/shockwave/download/index.cgi?P1_Prod_Version=ShockwaveFlash' type='application/x-shockwave-flash' width=500 height=400>$2</embed></OBJECT>");

            my = new Regex(@"(\[ZIP\])(.[^\[]*)(\[\/ZIP\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<br><IMG SRC=pic/zip.gif border=0> <a href=""$2"">点击下载该文件</a>");

            my = new Regex(@"(\[RAR\])(.[^\[]*)(\[\/RAR\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<br><IMG SRC=pic/rar.gif border=0> <a href=""$2"">点击下载该文件</a>");

            my = new Regex(@"(\[UPLOAD=(.[^\[]*)\])(.[^\[]*)(\[\/UPLOAD\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<br><IMG SRC=""pic/$2.gif"" border=0>此主题相关图片如下：<br><A HREF=""$3"" TARGET=_blank><IMG SRC=""$3"" border=0 alt=按此在新窗口浏览图片 onload=""javascript:if(this.width>screen.width-333)this.width=screen.width-333""></A>");

            my = new Regex(@"(\[URL\])(http:\/\/.[^\[]*)(\[\/URL\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<A HREF=""$2"" TARGET=_blank>$2</A>");

            my = new Regex(@"(\[URL\])(.[^\[]*)(\[\/URL\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<A HREF=""http://$2"" TARGET=_blank>$2</A>");

            my = new Regex(@"(\[URL=(http:\/\/.[^\[]*)\])(.[^\[]*)(\[\/URL\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<A HREF=""$2"" TARGET=_blank>$3</A>");

            my = new Regex(@"(\[URL=(.[^\[]*)\])(.[^\[]*)(\[\/URL\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<A HREF=""http://$2"" TARGET=_blank>$3</A>");

            my = new Regex(@"(\[EMAIL\])(\S+\@.[^\[]*)(\[\/EMAIL\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<A HREF=""mailto:$2"">$2</A>");

            my = new Regex(@"(\[EMAIL=(\S+\@.[^\[]*)\])(.[^\[]*)(\[\/EMAIL\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<A HREF=""mailto:$2"" TARGET=_blank>$3</A>");

            my = new Regex(@"^(HTTP://[A-Za-z0-9\./=\?%\-&_~`@':+!]+)", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<a target=_blank href=$1>$1</a>");

            my = new Regex(@"(HTTP://[A-Za-z0-9\./=\?%\-&_~`@':+!]+)$", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<a target=_blank href=$1>$1</a>");

            my = new Regex(@"[^>=""](HTTP://[A-Za-z0-9\./=\?%\-&_~`@':+!]+)", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<a target=_blank href=$1>$1</a>");

            my = new Regex(@"^(FTP://[A-Za-z0-9\./=\?%\-&_~`@':+!]+)", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<a target=_blank href=$1>$1</a>");

            my = new Regex(@"(FTP://[A-Za-z0-9\./=\?%\-&_~`@':+!]+)$", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<a target=_blank href=$1>$1</a>");

            my = new Regex(@"[^>=""](FTP://[A-Za-z0-9\.\/=\?%\-&_~`@':+!]+)", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<a target=_blank href=$1>$1</a>");

            my = new Regex(@"^(RTSP://[A-Za-z0-9\./=\?%\-&_~`@':+!]+)", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<a target=_blank href=$1>$1</a>");

            my = new Regex(@"(RTSP://[A-Za-z0-9\./=\?%\-&_~`@':+!]+)$", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<a target=_blank href=$1>$1</a>");

            my = new Regex(@"[^>=""](RTSP://[A-Za-z0-9\.\/=\?%\-&_~`@':+!]+)", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<a target=_blank href=$1>$1</a>");

            my = new Regex(@"^(MMS://[A-Za-z0-9\./=\?%\-&_~`@':+!]+)", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<a target=_blank href=$1>$1</a>");
            my = new Regex(@"(MMS://[A-Za-z0-9\./=\?%\-&_~`@':+!]+)$", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<a target=_blank href=$1>$1</a>");

            my = new Regex(@"[^>=""](MMS://[A-Za-z0-9\.\/=\?%\-&_~`@':+!]+)", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<a target=_blank href=$1>$1</a>");

            my = new Regex(@"(\[HTML\])(.[^\[]*)(\[\/HTML\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<table width='100%' border='0' cellspacing='0' cellpadding='6' bgcolor=''><td><b>以下内容为程序代码:</b><br>$2</td></table>");

            my = new Regex(@"(\[CODE\])(.[^\[]*)(\[\/CODE\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<table width='100%' border='0' cellspacing='0' cellpadding='6' bgcolor=''><td><b>以下内容为程序代码:</b><br>$2</td></table>");

            my = new Regex(@"(\[COLOR=(.[^\[]*)\])(.[^\[]*)(\[\/COLOR\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<font COLOR=$2>$3</font>");

            my = new Regex(@"(\[FACE=(.[^\[]*)\])(.[^\[]*)(\[\/FACE\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<font FACE=$2>$3</font>");

            my = new Regex(@"(\[ALIGN=(.[^\[]*)\])(.*)(\[\/ALIGN\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<div ALIGN=$2>$3</div>");

            my = new Regex(@"(\[QUOTE\])(.*)(\[\/QUOTE\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<table cellpadding=0 cellspacing=0 border=0 WIDTH=94% bgcolor=#000000 align=center><tr><td><table width=100% cellpadding=5 cellspacing=1 border=0><TR><TD BGCOLOR=''>$2</table></table><br>");

            my = new Regex(@"(\[MOVE\])(.*)(\[\/MOVE\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<MARQUEE scrollamount=3>$2</marquee>");

            my = new Regex(@"\[GLOW=*([0-9]*),*(#*[a-z0-9]*),*([0-9]*)\](.[^\[]*)\[\/GLOW]", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<table width=$1 style=""filter:glow(color=$2, strength=$3)"">$4</table>");

            my = new Regex(@"\[SHADOW=*([0-9]*),*(#*[a-z0-9]*),*([0-9]*)\](.[^\[]*)\[\/SHADOW]", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<table width=$1 style=""filter:shadow(color=$2, strength=$3)"">$4</table>");

            my = new Regex(@"(\[I\])(.[^\[]*)(\[\/I\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<i>$2</i>");

            my = new Regex(@"(\[B\])(.[^\[]*)(\[\/U\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<u>$2</u>");

            my = new Regex(@"(\[B\])(.[^\[]*)(\[\/B\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<b>$2</b>");

            my = new Regex(@"(\[FLY\])(.[^\[]*)(\[\/FLY\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<marquee onmouseover='this.stop();' onmouseout='this.start();'>$2</marquee>");

            my = new Regex(@"(\[SIZE=1\])(.[^\[]*)(\[\/SIZE\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<font size=1>$2</font>");

            my = new Regex(@"(\[SIZE=2\])(.[^\[]*)(\[\/SIZE\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<font size=2>$2</font>");

            my = new Regex(@"(\[SIZE=3\])(.[^\[]*)(\[\/SIZE\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<font size=3>$2</font>");

            my = new Regex(@"(\[SIZE=4\])(.[^\[]*)(\[\/SIZE\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<font size=4>$2</font>");

            my = new Regex(@"(\[CENTER\])(.[^\[]*)(\[\/CENTER\])", RegexOptions.IgnoreCase);
            ubbStr = my.Replace(ubbStr, @"<center>$2</center>");

            return ubbStr;
        }

        /// <summary>
        /// UBB转HTML方式2
        /// </summary>
        /// <param name="ubbStr">UBB 代码</param>
        /// <returns>HTML代码</returns>
        public static async Task<string> UbbToHtml2Async(this string ubbStr) => await Task.Run(() => UbbToHtml2(ubbStr));

        /// <summary>
        /// Html转UBB
        /// </summary>
        /// <param name="chr">HTML代码</param>
        /// <returns>UBB代码</returns>
        public static string HtmltoUBB(this string chr)
        {
            if (chr == null) return "";
            chr = chr.Replace("&lt", "<");
            chr = chr.Replace("&gt", ">");
            chr = chr.Replace("<br/>", " ");
            chr = Regex.Replace(chr, @"<a href=$1 target=_blank>$2</a>", @"[url=(?<x>[^]]*)](?<y>[^]]*)[/url]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<a href=$1 target=_blank>$1</a>", @"[url](?<x>[^]]*)[/url]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<a href=$1>$2</a>", @"[email=(?<x>[^]]*)](?<y>[^]]*)[/email]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<a href=$1>$1</a>", @"[email](?<x>[^]]*)[/email]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<OBJECT codeBase=http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=4,0,2,0 classid=clsid:D27CDB6E-AE6D-11cf-96B8-444553540000 width=500 height=400><PARAM NAME=movie VALUE=""$1""><PARAM NAME=quality VALUE=high><embed src=""$1"" quality=high pluginspage='http://www.macromedia.com/shockwave/download/index.cgi?P1_Prod_Version=ShockwaveFlash' type='application/x-shockwave-flash' width=500 height=400>$1</embed></OBJECT>", @"[flash](?<x>[^]]*)[/flash]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<IMG SRC=""$1"" border=0>", @"[img](?<x>[^]]*)[/img]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<font color=$1>$2</font>", @"[color=(?<x>[^]]*)](?<y>[^]]*)[/color]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<font face=$1>$2</font>", @"[face=(?<x>[^]]*)](?<y>[^]]*)[/face]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<font size=1>$1</font>", @"[size=1](?<x>[^]]*)[/size]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<font size=2>$1</font>", @"[size=2](?<x>[^]]*)[/size]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<font size=3>$1</font>", @"[size=3](?<x>[^]]*)[/size]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<font size=4>$1</font>", @"[size=4](?<x>[^]]*)[/size]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<align=$1>$2</align>", @"[align=(?<x>[^]]*)](?<y>[^]]*)[/align]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<marquee width=90% behavior=alternate scrollamount=3>$1</marquee>", @"[fly](?<x>[^]]*)[/fly]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<marquee scrollamount=3>$1</marquee>", @"[move](?<x>[^]]*)[/move]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<table width=$1 style='filter:glow(color=$2, strength=$3)'>$4</table>", @"[glow=(?<x>[^]]*),(?<y>[^]]*),(?<z>[^]]*)](?<w>[^]]*)[/glow]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<table width=$1 style='filter:shadow(color=$2, strength=$3)'>$4</table>", @"[shadow=(?<x>[^]]*),(?<y>[^]]*),(?<z>[^]]*)](?<w>[^]]*)[/shadow]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<b>$1</b>", @"[b](?<x>[^]]*)[/b]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<i>$1</i>", @"[i](?<x>[^]]*)[/i]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<u>$1</u>", @"[u](?<x>[^]]*)[/u]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<pre id=code><font size=1 face='Verdana, Arial' id=code>$1</font id=code></pre id=code>", @"[code](?<x>[^]]*)[/code]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<ul>$1</ul>", @"[list](?<x>[^]]*)[/list]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<ol type=1>$1</ol id=1>", @"[list=1](?<x>[^]]*)[/list]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<ol type=a>$1</ol id=a>", @"[list=a](?<x>[^]]*)[/list]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<li>$1</li>", @"[*](?<x>[^]]*)[/*]", RegexOptions.IgnoreCase);
            chr = Regex.Replace(chr, @"<center>—— 以下是引用 ——<table border='1' width='80%' cellpadding='10' cellspacing='0' ><tr><td>$1</td></tr></table></center>", @"[quote](?<x>.*)[/quote]", RegexOptions.IgnoreCase);
            return chr;
        }

        /// <summary>
        /// Html转UBB
        /// </summary>
        /// <param name="chr">HTML代码</param>
        /// <returns>UBB代码</returns>
        public static async Task<string> HtmltoUBBAsync(this string chr) => await Task.Run(() => HtmltoUBB(chr));

        #endregion

        #region 数字互转

        /// <summary>
        /// 字符串转int
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>int类型的数字</returns>
        public static int ToInt32(this string s)
        {
            bool b = int.TryParse(s, out int result);
            return result;
        }

        /// <summary>
        /// 字符串转long
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>int类型的数字</returns>
        public static long ToInt64(this string s)
        {
            bool b = long.TryParse(s, out var result);
            return result;
        }

        /// <summary>
        /// 字符串转double
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>double类型的数据</returns>
        public static double ToDouble(this string s)
        {
            bool b = double.TryParse(s, out var result);
            return result;
        }

        /// <summary>
        /// 字符串转decimal
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>int类型的数字</returns>
        public static decimal ToDecimal(this string s)
        {
            var b = decimal.TryParse(s, out var result);
            return result;
        }

        /// <summary>
        /// 字符串转decimal
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>int类型的数字</returns>
        public static decimal ToDecimal(this double s)
        {
            return new decimal(s);
        }

        /// <summary>
        /// 字符串转double
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>double类型的数据</returns>
        public static double ToDouble(this decimal s)
        {
            return (double)s;
        }

        /// <summary>
        /// 将double转换成int
        /// </summary>
        /// <param name="num">double类型</param>
        /// <returns>int类型</returns>
        public static int ToInt32(this double num)
        {
            return (int)Math.Floor(num);
        }

        /// <summary>
        /// 将double转换成int
        /// </summary>
        /// <param name="num">double类型</param>
        /// <returns>int类型</returns>
        public static int ToInt32(this decimal num)
        {
            return (int)Math.Floor(num);
        }

        /// <summary>
        /// 将int转换成double
        /// </summary>
        /// <param name="num">int类型</param>
        /// <returns>int类型</returns>
        public static double ToDouble(this int num)
        {
            return num * 1.0;
        }

        /// <summary>
        /// 将int转换成decimal
        /// </summary>
        /// <param name="num">int类型</param>
        /// <returns>int类型</returns>
        public static decimal ToDecimal(this int num)
        {
            return (decimal)(num * 1.0);
        }

        #endregion

        #region 检测字符串中是否包含列表中的关键词

        /// <summary>
        /// 检测字符串中是否包含列表中的关键词
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="keys">关键词列表</param>
        /// <returns></returns>
        public static bool Contains(this string s, IEnumerable<string> keys) => Regex.IsMatch(s.ToLower(), string.Join("|", keys).ToLower());

        #endregion

        #region 匹配Email

        /// <summary>
        /// 匹配Email
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="isMatch">是否匹配成功，若返回true，则会得到一个Match对象，否则为null</param>
        /// <returns>匹配对象</returns>
        public static Match MatchEmail(this string s, out bool isMatch)
        {
            Match match = Regex.Match(s, @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
            isMatch = match.Success;
            return isMatch ? match : null;
        }

        /// <summary>
        /// 匹配Email
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>是否匹配成功</returns>
        public static bool MatchEmail(this string s)
        {
            MatchEmail(s, out bool success);
            return success;
        }

        #endregion

        #region 匹配完整的URL

        /// <summary>
        /// 匹配完整格式的URL
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="isMatch">是否匹配成功，若返回true，则会得到一个Match对象，否则为null</param>
        /// <returns>匹配对象</returns>
        public static Uri MatchUrl(this string s, out bool isMatch)
        {
            try
            {
                isMatch = true;
                return new Uri(s);
            }
            catch (Exception e)
            {
                isMatch = false;
                return null;
            }
        }

        /// <summary>
        /// 匹配完整格式的URL
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>是否匹配成功</returns>
        public static bool MatchUrl(this string s)
        {
            MatchUrl(s, out var isMatch);
            return isMatch;
        }

        #endregion

        #region 权威校验身份证号码

        /// <summary>
        /// 根据GB11643-1999标准权威校验中国身份证号码的合法性
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>是否匹配成功</returns>
        public static bool MatchIdentifyCard(this string s)
        {
            if (s.Length == 18)
            {
                if (long.TryParse(s.Remove(17), out var n) == false || n < Math.Pow(10, 16) || long.TryParse(s.Replace('x', '0').Replace('X', '0'), out n) == false)
                {
                    return false; //数字验证  
                }

                string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
                if (address.IndexOf(s.Remove(2), StringComparison.Ordinal) == -1)
                {
                    return false; //省份验证  
                }

                string birth = s.Substring(6, 8).Insert(6, "-").Insert(4, "-");
                DateTime time;
                if (!DateTime.TryParse(birth, out time))
                {
                    return false; //生日验证  
                }

                string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
                string[] wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
                char[] ai = s.Remove(17).ToCharArray();
                int sum = 0;
                for (int i = 0; i < 17; i++)
                {
                    sum += wi[i].ToInt32() * ai[i].ToString().ToInt32();
                }

                int y;
                Math.DivRem(sum, 11, out y);
                if (arrVarifyCode[y] != s.Substring(17, 1).ToLower())
                {
                    return false; //校验码验证  
                }

                return true; //符合GB11643-1999标准  
            }

            if (s.Length == 15)
            {
                if (long.TryParse(s, out var n) == false || n < Math.Pow(10, 14))
                {
                    return false; //数字验证  
                }

                string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
                if (address.IndexOf(s.Remove(2), StringComparison.Ordinal) == -1)
                {
                    return false; //省份验证  
                }

                string birth = s.Substring(6, 6).Insert(4, "-").Insert(2, "-");
                if (DateTime.TryParse(birth, out _) == false)
                {
                    return false; //生日验证  
                }

                return true;
            }

            return false;
        }

        #endregion

        #region 校验IP地址的合法性

        /// <summary>
        /// 校验IP地址的正确性，同时支持IPv4和IPv6
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="isMatch">是否匹配成功，若返回true，则会得到一个Match对象，否则为null</param>
        /// <returns>匹配对象</returns>
        public static Match MatchInetAddress(this string s, out bool isMatch)
        {
            Match match;
            if (s.Contains(":"))
            {
                //IPv6
                match = Regex.Match(s, @"^([\da-fA-F]{0,4}:){1,7}[\da-fA-F]{1,4}$");
                isMatch = match.Success;
            }
            else
            {
                //IPv4
                match = Regex.Match(s, @"^(\d+)\.(\d+)\.(\d+)\.(\d+)$");
                isMatch = match.Success;
                foreach (Group m in match.Groups)
                {
                    if (m.Value.ToInt32() < 0 || m.Value.ToInt32() > 255)
                    {
                        isMatch = false;
                        break;
                    }
                }
            }

            return isMatch ? match : null;
        }

        /// <summary>
        /// 校验IP地址的正确性，同时支持IPv4和IPv6
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>是否匹配成功</returns>
        public static bool MatchInetAddress(this string s)
        {
            MatchInetAddress(s, out bool success);
            return success;
        }

        #endregion

        #region 校验手机号码的正确性

        /// <summary>
        /// 匹配手机号码
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="isMatch">是否匹配成功，若返回true，则会得到一个Match对象，否则为null</param>
        /// <returns>匹配对象</returns>
        public static Match MatchPhoneNumber(this string s, out bool isMatch)
        {
            Match match = Regex.Match(s, @"^((1[3,5,8][0-9])|(14[5,7])|(17[0,1,3,6,7,8])|(19[8,9]))\d{8}$");
            isMatch = match.Success;
            return isMatch ? match : null;
        }

        /// <summary>
        /// 匹配手机号码
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>是否匹配成功</returns>
        public static bool MatchPhoneNumber(this string s)
        {
            MatchPhoneNumber(s, out bool success);
            return success;
        }

        #endregion

        /// <summary>
        /// 严格比较两个对象是否是同一对象
        /// </summary>
        /// <param name="_this">自己</param>
        /// <param name="o">需要比较的对象</param>
        /// <returns>是否同一对象</returns>
        public new static bool ReferenceEquals(this object _this, object o) => object.ReferenceEquals(_this, o);

        /// <summary>
        /// 判断字符串是否为空
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);

        /// <summary>
        /// 类型直转
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T To<T>(this IConvertible value)
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// 字符串转时间
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string value)
        {
            DateTime.TryParse(value, out var result);
            return result;
        }

        /// <summary>
        /// 字符串转Guid
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Guid ToGuid(this string s)
        {
            return Guid.Parse(s);
        }

        /// <summary>
        /// 根据正则替换
        /// </summary>
        /// <param name="input"></param>
        /// <param name="regex">正则表达式</param>
        /// <param name="replacement">新内容</param>
        /// <returns></returns>
        public static string Replace(this string input, Regex regex, string replacement)
        {
            return regex.Replace(input, replacement);
        }

        /// <summary>
        /// 生成唯一短字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="chars">可用字符数数量，0-9,a-z,A-Z</param>
        /// <returns></returns>
        public static string CreateShortToken(this string str, byte chars = 36)
        {
            var nf = new NumberFormater(chars);
            return nf.ToString((DateTime.Now.Ticks - 630822816000000000) * 100 + Stopwatch.GetTimestamp() % 100);
        }

        /// <summary>
        /// 十进制转任意进制
        /// </summary>
        /// <param name="num"></param>
        /// <param name="bin">进制</param>
        /// <returns></returns>
        public static string ToBinary(this long num, int bin)
        {
            var nf = new NumberFormater(bin);
            return nf.ToString(num);
        }

        /// <summary>
        /// 十进制转任意进制
        /// </summary>
        /// <param name="num"></param>
        /// <param name="bin">进制</param>
        /// <returns></returns>
        public static string ToBinary(this int num, int bin)
        {
            var nf = new NumberFormater(bin);
            return nf.ToString(num);
        }

        /// <summary>
        /// 按字段去重
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> hash = new HashSet<TKey>();
            return source.Where(p => hash.Add(keySelector(p)));
        }

        /// <summary>
        /// 将小数截断为8位
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static double Digits8(this double num)
        {
            return (long)(num * 1E+8) * 1e-8;
        }

        /// <summary>
        /// 判断IP地址在不在某个IP地址段
        /// </summary>
        /// <param name="input">需要判断的IP地址</param>
        /// <param name="begin">起始地址</param>
        /// <param name="ends">结束地址</param>
        /// <returns></returns>
        public static bool IpAddressInRange(this string input, string begin, string ends)
        {
            uint current = IPToID(input);
            return current >= IPToID(begin) && current <= IPToID(ends);
        }

        /// <summary>
        /// IP地址转换成数字
        /// </summary>
        /// <param name="addr">IP地址</param>
        /// <returns>数字,输入无效IP地址返回0</returns>
        private static uint IPToID(string addr)
        {
            if (!IPAddress.TryParse(addr, out var ip))
            {
                return 0;
            }

            byte[] bInt = ip.GetAddressBytes();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bInt);
            }

            return BitConverter.ToUInt32(bInt, 0);
        }

        /// <summary>
        /// 判断IP是否是私有地址
        /// </summary>
        /// <param name="myIPAddress"></param>
        /// <returns></returns>
        public static bool IsPrivateIP(this IPAddress myIPAddress)
        {
            if (IPAddress.IsLoopback(myIPAddress)) return true;
            if (myIPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                byte[] ipBytes = myIPAddress.GetAddressBytes();
                // 10.0.0.0/24 
                if (ipBytes[0] == 10)
                {
                    return true;
                }
                // 169.254.0.0/16
                if (ipBytes[0] == 169 && ipBytes[1] == 254)
                {
                    return true;
                }
                // 172.16.0.0/16
                if (ipBytes[0] == 172 && ipBytes[1] == 16)
                {
                    return true;
                }
                // 192.168.0.0/16
                if (ipBytes[0] == 192 && ipBytes[1] == 168)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断IP是否是私有地址
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsPrivateIP(this string ip)
        {
            if (MatchInetAddress(ip))
            {
                return IsPrivateIP(IPAddress.Parse(ip));
            }
            throw new ArgumentException("不是一个合法的ip地址");
        }

        /// <summary>
        /// 判断url是否是外部地址
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsExternalAddress(this string url)
        {
            var uri = new Uri(url);
            switch (uri.HostNameType)
            {
                case UriHostNameType.Dns:
                    var ipHostEntry = Dns.GetHostEntry(uri.DnsSafeHost);
                    foreach (IPAddress ipAddress in ipHostEntry.AddressList)
                    {
                        if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            if (!ipAddress.IsPrivateIP())
                            {
                                return true;
                            }
                        }
                    }
                    break;

                case UriHostNameType.IPv4:
                    return !IPAddress.Parse(uri.DnsSafeHost).IsPrivateIP();
            }
            return false;
        }

        /// <summary>
        /// 生成真正的随机数
        /// </summary>
        /// <param name="r"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static int StrictNext(this Random r, int seed = int.MaxValue)
        {
            return new Random((int)Stopwatch.GetTimestamp()).Next(seed);
        }

        /// <summary>
        /// 产生正态分布的随机数
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="mean">均值</param>
        /// <param name="stdDev">方差</param>
        /// <returns></returns>
        public static double NextGauss(this Random rand, double mean, double stdDev)
        {
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + stdDev * randStdNormal;
        }
    }
}