using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Masuit.Tools.Logging;

namespace Masuit.Tools.Net
{
    /// <summary>
    ///  Cookie操作辅助类
    /// </summary>
    public static class CookieHelper
    {
        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetGetCookie(string url, string cookieName, StringBuilder cookieData, ref int size);

        /// <summary>
        /// 获取Cookie容器
        /// </summary>
        /// <param name="uri">URL</param>
        /// <returns>Cookie容器</returns>
        public static CookieContainer GetUriCookieContainer(this Uri uri)
        {
            CookieContainer cookies = null;
            //定义Cookie数据的大小。   
            int datasize = 256;
            StringBuilder cookieData = new StringBuilder(datasize);
            if (!InternetGetCookie(uri.ToString(), null, cookieData, ref datasize))
            {
                if (datasize < 0) return null;
                // 确信有足够大的空间来容纳Cookie数据。   
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookie(uri.ToString(), null, cookieData, ref datasize)) return null;
            }
            if (cookieData.Length > 0)
            {
                cookies = new CookieContainer();
                cookies.SetCookies(uri, cookieData.ToString().Replace(';', ','));
            }
            return cookies;
        }

        /// <summary>
        /// 获取url下的Cookie
        /// </summary>
        /// <param name="cookies">Cookie对象</param>
        /// <param name="uri">URL</param>
        /// <returns>Cookie内容</returns>
        public static string PrintCookies(this CookieContainer cookies, Uri uri)
        {
            CookieCollection cc = cookies.GetCookies(uri);
            StringBuilder sb = new StringBuilder();
            foreach (Cookie cook in cc)
            {
                sb.AppendLine(string.Format("{0}:{1}", cook.Name, cook.Value));
            }
            return sb.ToString();
        }

        #region 清除CooKie 

        #region 清除指定Cookie
        /// <summary>
        /// 清除指定Cookie
        /// </summary>
        /// <param name="cookiename">cookiename</param>
        public static void ClearCookie(string cookiename)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[cookiename];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddYears(-3);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }
        #endregion

        #region   删除所有cookie值
        /// <summary>
        /// 删除所有cookie值
        /// </summary>
        public static void Clear()
        {
            int n = HttpContext.Current.Response.Cookies.Count;
            for (int i = 0; i < n; i++)
            {
                HttpCookie MyCookie = HttpContext.Current.Response.Cookies[i];
                MyCookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(MyCookie);
            }
        }
        #endregion

        #region 清除系统指定的Cookie
        /// <summary>
        /// 清除系统指定的Cookie
        /// </summary>
        /// <param name="ck">指定的Cookie</param>
        /// <param name="url">Cookie的域</param>
        public static void ClearCookies(string ck, string url)
        {
            string[] cks = GetCKS(ck);
            List<string> domains = GetDomains(url);
            for (int i = 0; i < cks.Length; i++)
            {
                string[] nv = cks[i].Split('=');
                for (int j = 0; j < domains.Count; j++)
                {
                    InternetSetCookie(domains[j], nv[0], "abc;expires = Sat, 31-Dec-2007 14:00:00 GMT");
                }
            }
        }
        #endregion

        #region 清除系统的Cookie文件

        /// <summary>
        /// 清除系统的Cookie文件
        /// </summary>
        public static void ClearCookiesFiles()
        {
            DirectoryInfo di = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Cookies));
            foreach (FileInfo file in di.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch (Exception e)
                {
                    LogManager.Error(e);
                }
            }
        }
        #endregion
        #endregion

        #region 获取Cookie

        #region 获取指定Cookie值

        /// <summary>
        /// 获取指定Cookie值
        /// </summary>
        /// <param name="cookiename">cookiename</param>
        /// <returns>Cookie值</returns>
        public static string GetCookieValue(string cookiename)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[cookiename];
            string str = String.Empty;
            if (cookie != null)
            {
                str = cookie.Value;
            }
            return str;
        }

        /// <summary>
        /// 根据Key值得到Cookie值,Key不区分大小写
        /// </summary>
        /// <param name="Key">key</param>
        /// <param name="cookie">字符串Cookie</param>
        /// <returns>Cookie值</returns>
        public static string GetCookieValue(string Key, string cookie)
        {
            foreach (CookieItem item in GetCookieList(cookie))
            {
                if (item.Key == Key)
                    return item.Value;
            }
            return "";
        }

        /// <summary>
        /// 根据字符生成Cookie列表
        /// </summary>
        /// <param name="cookie">Cookie字符串</param>
        /// <returns>Cookie列表</returns>
        public static List<CookieItem> GetCookieList(string cookie)
        {
            List<CookieItem> cookielist = new List<CookieItem>();
            foreach (string item in cookie.Split(new[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (Regex.IsMatch(item, @"([\s\S]*?)=([\s\S]*?)$"))
                {
                    Match m = Regex.Match(item, @"([\s\S]*?)=([\s\S]*?)$");
                    cookielist.Add(new CookieItem() { Key = m.Groups[1].Value, Value = m.Groups[2].Value });
                }
            }
            return cookielist;
        }
        #endregion

        #region  获取cookie数组

        /// <summary>
        /// 获取cookie数组
        /// </summary>
        /// <param name="ck">cokki数据</param>
        /// <returns>字符串数组</returns>
        public static string[] GetCKS(string ck)
        {
            if (ck != null)
            {
                return ck.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                return new string[0];
            }
        }
        #endregion

        #region  从Cookie数组中转换成不重复的Cookie字符串，相同的Cookie取前面的

        /// <summary>
        /// 从Cookie数组中转换成不重复的Cookie字符串，相同的Cookie取前面的
        /// </summary>
        /// <param name="cks">Cookie数组</param>
        /// <returns>剔除重复的Cookie字符串</returns>
        public static string GetCK(string[] cks)
        {
            string res = "";
            List<string> list = new List<string>();
            for (int i = 0; i < cks.Length; i++)
            {
                if (cks[i].Trim() != "")
                {
                    if (!IncludeCKAsync(list, cks[i]).Result)
                    {
                        list.Add(cks[i].Trim());
                    }
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                res += list[i] + ";";
            }
            return res;
        }
        #endregion

        #region  从CookieCollection中获取Cookie字符串

        /// <summary>
        /// 从CookieCollection中获取Cookie字符串
        /// </summary>
        /// <param name="cc">CookieCollection，一般用在WebRequest中</param>
        /// <returns>转换后的Cookie字符串</returns>
        public static string GetCK(this CookieCollection cc)
        {
            string ck = "";
            for (int i = 0; i < cc.Count; i++)
            {
                ck += cc[i].Name + "=" + cc[i].Value + ";";
            }
            return ck;
        }
        #endregion

        #region   将Cookie字符串填充到CookieCollection中

        /// <summary>
        /// 将Cookie字符串填充到CookieCollection中
        /// </summary>
        /// <param name="ck">Cookie字符串</param>
        /// <param name="url">Cookie的域</param>
        /// <returns>添加后的CookieCollection</returns>
        public static CookieCollection GetCK(string ck, string url)
        {
            CookieCollection cc = new CookieCollection();
            string domain = "";
            try
            {
                Uri u = new Uri(url);
                domain = u.Host;
            }
            catch (Exception e)
            {
                LogManager.Error(e);
            }
            string[] cks = GetCKS(ck);
            for (int i = 0; i < cks.Length; i++)
            {
                if (cks[i].IndexOf("=") > -1)
                {
                    try
                    {
                        string n = cks[i].Substring(0, cks[i].IndexOf("="));
                        string v = cks[i].Substring(cks[i].IndexOf("=") + 1);
                        System.Net.Cookie c = new System.Net.Cookie();
                        c.Name = n.Trim();
                        c.Value = v.Trim();
                        c.Domain = domain;
                        cc.Add(c);
                    }
                    catch (Exception e)
                    {
                        LogManager.Error(e);
                    }
                }
            }
            return cc;
        }
        #endregion
        #endregion

        #region 添加Cookie

        #region   添加一个Cookie（24小时过期）

        /// <summary>
        /// 添加一个Cookie（24小时过期）
        /// </summary>
        /// <param name="cookiename">键</param>
        /// <param name="cookievalue">值</param>
        public static void SetCookie(string cookiename, string cookievalue)
        {
            SetCookie(cookiename, cookievalue, DateTime.Now.AddDays(1.0));
        }
        #endregion

        #region   添加一个Cookie
        /// <summary>
        /// 添加一个Cookie
        /// </summary>
        /// <param name="cookiename">cookie名</param>
        /// <param name="cookievalue">cookie值</param>
        /// <param name="expires">过期时间 DateTime</param>
        public static void SetCookie(string cookiename, string cookievalue, DateTime expires)
        {
            HttpCookie cookie = new HttpCookie(cookiename)
            {
                Value = cookievalue,
                Expires = expires
            };
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
        #endregion
        #endregion

        #region 检查Cookie集合中是否包含指定的Cookie值

        /// <summary>
        /// 检查Cookie集合中是否包含指定的Cookie值
        /// </summary>
        /// <param name="cks">Cookie集合</param>
        /// <param name="ck">待判断的Cookie</param>
        /// <returns>Cookie集合中是否包含指定的Cookie</returns>
        public static async Task<bool> IncludeCKAsync(this List<string> cks, string ck)
        {
            try
            {
                return await Task.Run(() =>
                {
                    foreach (string t in cks)
                    {
                        if (t.ToLower().Trim().IndexOf(ck.Remove(ck.IndexOf('=') + 1).Trim().ToLower(), StringComparison.Ordinal) != -1)
                        {
                            return true;
                        }
                    }
                    return false;
                }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                return false;
            }
        }
        #endregion

        #region 设置系统Cookie

        /// <summary>
        /// 设置系统Cookie
        /// </summary>
        /// <param name="lpszUrlName">Cookie域</param>
        /// <param name="lbszCookieName">Cookie名</param>
        /// <param name="lpszCookieData">Cookie数据</param>
        /// <returns>设置成功与否</returns>
        [System.Runtime.InteropServices.DllImport("wininet.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetCookie(string lpszUrlName, string lbszCookieName, string lpszCookieData);
        #endregion

        #region   获取所有可能的Cookie域

        /// <summary>
        /// 获取所有可能的Cookie域
        /// </summary>
        /// <param name="url">域的全称</param>
        /// <returns>所有可能的域</returns>
        public static List<string> GetDomains(string url)
        {
            List<string> res = new List<string>();
            try
            {
                url = url.Remove(url.IndexOf("?"));
            }
            catch (Exception e)
            {
                LogManager.Error(e);
            }
            try
            {
                Uri uri = new Uri(url);
                string baseDomain = uri.Scheme + "://" + uri.Host;
                for (int i = 0; i < uri.Segments.Length; i++)
                {
                    baseDomain += uri.Segments[i];
                    res.Add(baseDomain);
                }
            }
            catch (Exception e)
            {
                LogManager.Error(e);
            }
            return res;
        }
        #endregion

        #region  将Cookie字符串描述的Cookie追加到CookieCoollection

        /// <summary>
        /// 将Cookie字符串描述的Cookie追加到CookieCoollection
        /// </summary>
        /// <param name="cc">CookieCoollection</param>
        /// <param name="ck">Cookie字符串</param>
        /// <param name="url">Cookie的域</param>
        public static void SetCKAppendToCC(this CookieCollection cc, string ck, string url)
        {
            CookieCollection tmp = GetCK(ck, url);
            for (int i = 0; i < tmp.Count; i++)
            {
                cc.Add(tmp[i]);
            }
        }
        #endregion

        #region 将Cookie字符串设置到系统中，便于浏览器使用

        /// <summary>
        /// 将Cookie字符串设置到系统中，便于浏览器使用
        /// </summary>
        /// <param name="ck">Cookie字符串</param>
        /// <param name="url">Cookie的域</param>
        public static void SetCKToSystem(string ck, string url)
        {
            string[] cks = GetCKS(ck);
            for (int i = 0; i < cks.Length; i++)
            {
                string[] nv = cks[i].Split('=');
                InternetSetCookie(url, nv[0], nv.Length > 1 ? nv[1] : "");
            }
        }
        #endregion

        #region 将CookieCollection中的Cookie设置到系统中，便于浏览器使用
        /// <summary>
        /// 将CookieCollection中的Cookie设置到系统中，便于浏览器使用
        /// </summary>
        /// <param name="cc">CookieCollection</param>
        /// <param name="url">Cookie的域</param>
        public static void SetCKToSystem(this CookieCollection cc, string url)
        {
            List<string> domains = GetDomains(url);
            for (int i = 0; i < cc.Count; i++)
            {
                for (int j = 0; j < domains.Count; j++)
                {
                    InternetSetCookie(domains[j], cc[i].Name, cc[i].Value);
                }
            }
        }
        #endregion

        /// <summary>
        /// 格式化Cookie为标准格式
        /// </summary>
        /// <param name="key">Key值</param>
        /// <param name="value">Value值</param>
        /// <returns>格式化后的数据</returns>
        public static string CookieFormat(string key, string value)
        {
            return string.Format("{0}={1};", key, value);
        }
    }

    /// <summary>
    /// Cookie对象
    /// </summary>
    public class CookieItem
    {
        /// <summary>
        /// 键
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }
    }
}
