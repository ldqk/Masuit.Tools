using System;
using System.Web;

namespace Masuit.Tools.Net
{
    /// <summary>
    ///  Cookie操作辅助类
    /// </summary>
    public static class CookieHelper
    {
        /// <summary>
        /// 清除指定Cookie
        /// </summary>
        /// <param name="cookiename">cookiename</param>
        public static void Clear(string cookiename)
        {
            var cookie = HttpContext.Current.Request.Cookies[cookiename];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddYears(-3);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        /// <summary>
        /// 删除所有cookie值
        /// </summary>
        public static void ClearAll()
        {
            int n = HttpContext.Current.Response.Cookies.Count;
            for (int i = 0; i < n; i++)
            {
                var myCookie = HttpContext.Current.Response.Cookies[i];
                myCookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(myCookie);
            }
        }

        /// <summary>
        /// 获取指定Cookie值
        /// </summary>
        /// <param name="cookiename">cookiename</param>
        /// <returns>Cookie值</returns>
        public static string GetCookieValue(string cookiename)
        {
            var cookie = HttpContext.Current.Request.Cookies[cookiename];
            string str = null;
            if (cookie != null)
            {
                str = cookie.Value;
            }

            return str;
        }

        /// <summary>
        /// 添加一个Cookie
        /// </summary>
        /// <param name="cookiename">cookie名</param>
        /// <param name="cookievalue">cookie值</param>
        /// <param name="expires">过期时间 DateTime</param>
        public static void SetCookie(string cookiename, string cookievalue, DateTime expires)
        {
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(cookiename)
            {
                Value = cookievalue,
                Expires = expires
            });
        }
    }
}