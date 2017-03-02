using System;
using System.Text.RegularExpressions;

namespace Masuit.Tools.Html
{
    /// <summary>
    /// HTML工具类
    /// </summary>
    public static class HtmlTools
    {
        #region 获得发表日期、编码
        /// <summary>
        /// 获得发表日期、编码
        /// </summary>
        /// <param name="sContent">内容</param>
        /// <param name="sRegex">正则表达式</param>
        /// <returns>日期</returns>
        public static DateTime GetCreateDate(string sContent, string sRegex)
        {
            DateTime dt = System.DateTime.Now;

            Regex re = new Regex(sRegex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            Match mc = re.Match(sContent);
            if (mc.Success)
            {
                try
                {
                    int iYear = int.Parse(mc.Groups["Year"].Value);
                    int iMonth = int.Parse(mc.Groups["Month"].Value);
                    int iDay = int.Parse(mc.Groups["Day"].Value);
                    int iHour = dt.Hour;
                    int iMinute = dt.Minute;

                    string sHour = mc.Groups["Hour"].Value;
                    string sMintue = mc.Groups["Mintue"].Value;

                    if (sHour != "")
                        iHour = int.Parse(sHour);
                    if (sMintue != "")
                        iMinute = int.Parse(sMintue);

                    dt = new DateTime(iYear, iMonth, iDay, iHour, iMinute, 0);
                }
                catch
                {
                }
            }
            return dt;
        }
        #endregion 获得发表日期

    }
}
