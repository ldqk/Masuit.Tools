using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using Ganss.XSS;
using Masuit.Tools.Logging;

namespace Masuit.Tools.Html
{
    /// <summary>
    /// html工具类
    /// </summary>
    public static partial class HtmlTools
    {
        #region 防止html的xss净化器
        /// <summary>
        /// 标准的防止html的xss净化器
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HtmlSantinizerStandard(this string html)
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedAttributes.Remove("id");
            sanitizer.AllowedAttributes.Remove("alt");
            sanitizer.AllowedCssProperties.Remove("font-family");
            sanitizer.AllowedCssProperties.Remove("background-color");
            sanitizer.KeepChildNodes = true;
            sanitizer.AllowedTags.Remove("input");
            sanitizer.AllowedTags.Remove("button");
            sanitizer.AllowedTags.Remove("iframe");
            sanitizer.AllowedTags.Remove("frame");
            sanitizer.AllowedTags.Remove("textarea");
            sanitizer.AllowedTags.Remove("select");
            sanitizer.AllowedTags.Remove("form");
            return sanitizer.Sanitize(html);
        }

        /// <summary>
        /// 自定义的防止html的xss净化器
        /// </summary>
        /// <param name="html">源html</param>
        /// <param name="labels">需要移除的标签集合</param>
        /// <param name="attributes">需要移除的属性集合</param>
        /// <param name="styles">需要移除的样式集合</param>
        /// <returns></returns>
        public static string HtmlSantinizerCustom(this string html, string[] labels = null, string[] attributes = null, string[] styles = null)
        {
            var sanitizer = new HtmlSanitizer();
            if (labels != null)
            {
                foreach (string label in labels)
                {
                    sanitizer.AllowedTags.Remove(label);
                }
            }

            if (attributes != null)
            {
                foreach (string attr in attributes)
                {
                    sanitizer.AllowedAttributes.Remove(attr);
                }
            }

            if (styles != null)
            {
                foreach (string p in styles)
                {
                    sanitizer.AllowedCssProperties.Remove(p);
                }
            }
            sanitizer.KeepChildNodes = true;
            return sanitizer.Sanitize(html);
        }
        #endregion
        #region BaseMethod
        /// <summary>
        /// 多个匹配内容
        /// </summary>
        /// <param name="sInput">输入内容</param>
        /// <param name="sRegex">表达式字符串</param>
        /// <param name="iGroupIndex">第几个分组, 从1开始, 0代表不分组</param>
        public static List<string> GetList(string sInput, string sRegex, int iGroupIndex)
        {
            List<string> list = new List<string>();
            Regex re = new Regex(sRegex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            MatchCollection mcs = re.Matches(sInput);
            foreach (Match mc in mcs)
            {
                if (iGroupIndex > 0)
                {
                    list.Add(mc.Groups[iGroupIndex].Value);
                }
                else
                {
                    list.Add(mc.Value);
                }
            }
            return list;
        }

        /// <summary>
        /// 多个匹配内容
        /// </summary>
        /// <param name="sInput">输入内容</param>
        /// <param name="sRegex">表达式字符串</param>
        /// <param name="sGroupName">分组名, ""代表不分组</param>
        public static List<string> GetList(string sInput, string sRegex, string sGroupName)
        {
            List<string> list = new List<string>();
            Regex re = new Regex(sRegex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            MatchCollection mcs = re.Matches(sInput);
            foreach (Match mc in mcs)
            {
                if (sGroupName != "")
                {
                    list.Add(mc.Groups[sGroupName].Value);
                }
                else
                {
                    list.Add(mc.Value);
                }
            }
            return list;
        }

        /// <summary>
        /// 单个匹配内容
        /// </summary>
        /// <param name="sInput">输入内容</param>
        /// <param name="sRegex">表达式字符串</param>
        /// <param name="iGroupIndex">分组序号, 从1开始, 0不分组</param>
        public static string GetText(string sInput, string sRegex, int iGroupIndex)
        {
            Regex re = new Regex(sRegex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            Match mc = re.Match(sInput);
            string result = "";
            if (mc.Success)
            {
                if (iGroupIndex > 0)
                {
                    result = mc.Groups[iGroupIndex].Value;
                }
                else
                {
                    result = mc.Value;
                }
            }
            return result;
        }

        /// <summary>
        /// 单个匹配内容
        /// </summary>
        /// <param name="sInput">输入内容</param>
        /// <param name="sRegex">表达式字符串</param>
        /// <param name="sGroupName">分组名, ""代表不分组</param>
        public static string GetText(string sInput, string sRegex, string sGroupName)
        {
            Regex re = new Regex(sRegex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            Match mc = re.Match(sInput);
            string result = "";
            if (mc.Success)
            {
                if (sGroupName != "")
                {
                    result = mc.Groups[sGroupName].Value;
                }
                else
                {
                    result = mc.Value;
                }
            }
            return result;
        }

        /// <summary>
        /// 替换指定内容
        /// </summary>
        /// <param name="sInput">输入内容</param>
        /// <param name="sRegex">表达式字符串</param>
        /// <param name="sReplace">替换值</param>
        /// <param name="iGroupIndex">分组序号, 0代表不分组</param>
        public static string Replace(string sInput, string sRegex, string sReplace, int iGroupIndex)
        {
            Regex re = new Regex(sRegex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            MatchCollection mcs = re.Matches(sInput);
            foreach (Match mc in mcs)
            {
                if (iGroupIndex > 0)
                {
                    sInput = sInput.Replace(mc.Groups[iGroupIndex].Value, sReplace);
                }
                else
                {
                    sInput = sInput.Replace(mc.Value, sReplace);
                }
            }
            return sInput;
        }

        /// <summary>
        /// 替换指定内容
        /// </summary>
        /// <param name="sInput">输入内容</param>
        /// <param name="sRegex">表达式字符串</param>
        /// <param name="sReplace">替换值</param>
        /// <param name="sGroupName">分组名, "" 代表不分组</param>
        public static string Replace(string sInput, string sRegex, string sReplace, string sGroupName)
        {
            Regex re = new Regex(sRegex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            MatchCollection mcs = re.Matches(sInput);
            foreach (Match mc in mcs)
            {
                if (sGroupName != "")
                {
                    sInput = sInput.Replace(mc.Groups[sGroupName].Value, sReplace);
                }
                else
                {
                    sInput = sInput.Replace(mc.Value, sReplace);
                }
            }
            return sInput;
        }

        /// <summary>
        /// 分割指定内容
        /// </summary>
        /// <param name="sInput">输入内容</param>
        /// <param name="sRegex">表达式字符串</param>
        /// <param name="iStrLen">最小保留字符串长度</param>
        public static List<string> Split(string sInput, string sRegex, int iStrLen)
        {
            Regex re = new Regex(sRegex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            string[] sArray = re.Split(sInput);
            List<string> list = new List<string>();
            list.Clear();
            foreach (string s in sArray)
            {
                if (s.Trim().Length < iStrLen)
                    continue;

                list.Add(s.Trim());
            }
            return list;
        }

        #endregion BaseMethod

        #region 获得特定内容

        /// <summary>
        /// 多个链接
        /// </summary>
        /// <param name="sInput">输入内容</param>
        public static List<string> GetLinks(string sInput)
        {
            return GetList(sInput, @"<a[^>]+href=\s*(?:'(?<href>[^']+)'|""(?<href>[^""]+)""|(?<href>[^>\s]+))\s*[^>]*>", "href");
        }

        /// <summary>
        /// 单个链接
        /// </summary>
        /// <param name="sInput">输入内容</param>
        public static string GetLinkHelp(string sInput)
        {
            return GetText(sInput, @"<a[^>]+href=\s*(?:'(?<href>[^']+)'|""(?<href>[^""]+)""|(?<href>[^>\s]+))\s*[^>]*>", "href");
        }

        /// <summary>
        /// 图片标签
        /// </summary>
        /// <param name="sInput">输入内容</param>
        public static List<string> GetImgTag(string sInput)
        {
            return GetList(sInput, "<img[^>]+src=\\s*(?:'(?<src>[^']+)'|\"(?<src>[^\"]+)\"|(?<src>[^>\\s]+))\\s*[^>]*>", "");
        }

        /// <summary>
        /// 图片地址
        /// </summary>
        /// <param name="sInput">输入内容</param>
        public static string GetImgSrc(string sInput)
        {
            return GetText(sInput, "<img[^>]+src=\\s*(?:'(?<src>[^']+)'|\"(?<src>[^\"]+)\"|(?<src>[^>\\s]+))\\s*[^>]*>", "src");
        }

        /// <summary>
        /// 根据URL获得域名
        /// </summary>
        /// <param name="sInput">输入内容</param>
        public static string GetDomain(string sInput)
        {
            return GetText(sInput, @"http(s)?://([\w-]+\.)+(\w){2,}", 0);
        }

        #endregion 获得特定内容

        #region 根据表达式，获得文章内容
        /// <summary>
        /// 文章标题
        /// </summary>
        /// <param name="sInput">输入内容</param>
        /// <param name="sRegex">表达式字符串</param>
        public static string GetTitle(string sInput, string sRegex)
        {
            string sTitle = GetText(sInput, sRegex, "Title");
            sTitle = ClearTag(sTitle);
            if (sTitle.Length > 99)
            {
                sTitle = sTitle.Substring(0, 99);
            }
            return sTitle;
        }

        /// <summary>
        /// 网页标题
        /// </summary>
        /// <param name="sInput">html</param>
        public static string GetTitle(string sInput)
        {
            return GetText(sInput, @"<Title[^>]*>(?<Title>[\s\S]{10,})</Title>", "Title");
        }

        /// <summary>
        /// 网页内容
        /// </summary>
        /// <param name="sInput">输入内容</param>
        public static string GetHtml(string sInput)
        {
            return Replace(sInput, @"(?<Head>[^<]+)<", "", "Head");
        }

        /// <summary>
        /// 网页Body内容
        /// </summary>
        /// <param name="sInput">html</param>
        public static string GetBodyHelp(string sInput)
        {
            return GetText(sInput, @"<Body[^>]*>(?<Body>[\s\S]{10,})</body>", "Body");
        }

        /// <summary>
        /// 网页Body内容
        /// </summary>
        /// <param name="sInput">输入内容</param>
        /// <param name="sRegex">表达式字符串</param>
        public static string GetBody(string sInput, string sRegex)
        {
            return GetText(sInput, sRegex, "Body");
        }

        /// <summary>
        /// 文章来源
        /// </summary>
        /// <param name="sInput">输入内容</param>
        /// <param name="sRegex">表达式字符串</param>
        public static string GetSource(string sInput, string sRegex)
        {
            string sSource = GetText(sInput, sRegex, "Source");
            sSource = ClearTag(sSource);
            if (sSource.Length > 99)
                sSource = sSource.Substring(0, 99);
            return sSource;
        }

        /// <summary>
        /// 作者名
        /// </summary>
        /// <param name="sInput">输入内容</param>
        /// <param name="sRegex">表达式字符串</param>
        public static string GetAuthor(string sInput, string sRegex)
        {
            string sAuthor = GetText(sInput, sRegex, "Author");
            sAuthor = ClearTag(sAuthor);
            if (sAuthor.Length > 99)
                sAuthor = sAuthor.Substring(0, 99);
            return sAuthor;
        }

        /// <summary>
        /// 分页链接地址
        /// </summary>
        /// <param name="sInput">输入内容</param>
        /// <param name="sRegex">表达式字符串</param>
        public static List<string> GetPageLinks(string sInput, string sRegex)
        {
            return GetList(sInput, sRegex, "href");
        }

        /// <summary>
        /// 根据相对路径得到绝对路径
        /// </summary>
        /// <param name="sInput">原始网站地址</param>
        /// <param name="sRelativeUrl">相对链接地址</param>
        public static string GetUrl(string sInput, string sRelativeUrl)
        {
            string sReturnUrl = "";
            string sUrl = _GetStandardUrlDepth(sInput);//返回了http://www.163.com/news/这种形式

            if (sRelativeUrl.ToLower().StartsWith("http") || sRelativeUrl.ToLower().StartsWith("https"))
            {
                sReturnUrl = sRelativeUrl.Trim();
            }
            else if (sRelativeUrl.StartsWith("/"))
            {
                sReturnUrl = GetDomain(sInput) + sRelativeUrl;
            }
            else if (sRelativeUrl.StartsWith("../"))
            {
                sUrl = sUrl.Substring(0, sUrl.Length - 1);
                while (sRelativeUrl.IndexOf("../") >= 0)
                {
                    string temp = sUrl.Substring(0, sUrl.LastIndexOf("/")); // CString.GetPreStrByLast(sUrl, "/");
                    if (temp.Length > 6)
                    {//temp != "http:/"，否则的话，说明已经回溯到尽头了，"../"与网址的层次对应不上。存在这种情况，网页上面的链接是错误的，但浏览器还能正常显示
                        sUrl = temp;
                    }
                    sRelativeUrl = sRelativeUrl.Substring(3);
                }
                sReturnUrl = sUrl + "/" + sRelativeUrl.Trim();
            }
            else if (sRelativeUrl.StartsWith("./"))
            {
                sReturnUrl = sUrl + sRelativeUrl.Trim().Substring(2);
            }
            else if (sRelativeUrl.Trim() != "")
            {//2007images/modecss.css
                sReturnUrl = sUrl + sRelativeUrl.Trim();
            }
            return sReturnUrl;
        }

        /// <summary>
        /// 获得标准的URL路径深度
        /// </summary>
        /// <param name="url">URL路径</param>
        /// <returns>返回标准的形式：http://www.163.com/或http://www.163.com/news/。</returns>
        private static string _GetStandardUrlDepth(string url)
        {
            string sheep = url.Trim().ToLower();
            string header = "http://";
            if (sheep.IndexOf("https://") != -1)
            {
                header = "https://";
                sheep = sheep.Replace("https://", "");
            }
            else
            {
                sheep = sheep.Replace("http://", "");
            }

            int p = sheep.LastIndexOf("/");
            if (p == -1)
            {//www.163.com
                sheep += "/";
            }
            else if (p == sheep.Length - 1)
            {//传来的是：http://www.163.com/news/
            }
            else if (sheep.Substring(p).IndexOf(".") != -1)
            {//传来的是：http://www.163.com/news/hello.htm 这种形式
                sheep = sheep.Substring(0, p + 1);
            }
            else
            {
                sheep += "/";
            }

            return header + sheep;
        }

        /// <summary>
        /// 关键字
        /// </summary>
        /// <param name="sInput">输入内容</param>
        public static string GetKeyWord(string sInput)
        {
            List<string> list = Split(sInput, "(,|，|\\+|＋|。|;|；|：|:|“)|”|、|_|\\(|（|\\)|）", 2);
            List<string> listReturn = new List<string>();
            Regex re;
            foreach (string str in list)
            {
                re = new Regex(@"[a-zA-z]+", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                MatchCollection mcs = re.Matches(str);
                string sTemp = str;
                foreach (Match mc in mcs)
                {
                    if (mc.Value.Length > 2)
                        listReturn.Add(mc.Value);
                    sTemp = sTemp.Replace(mc.Value, ",");
                }
                re = new Regex(@",{1}", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                mcs = re.Matches(sTemp);
                foreach (string s in re.Split(sTemp))
                {
                    if (s.Trim().Length <= 2)
                        continue;
                    listReturn.Add(s);
                }
            }
            string sReturn = "";
            for (int i = 0; i < listReturn.Count - 1; i++)
            {
                for (int j = i + 1; j < listReturn.Count; j++)
                {
                    if (listReturn[i] == listReturn[j])
                    {
                        listReturn[j] = "";
                    }
                }
            }
            foreach (string str in listReturn)
            {
                if (str.Length > 2)
                    sReturn += str + ",";
            }
            if (sReturn.Length > 0)
                sReturn = sReturn.Substring(0, sReturn.Length - 1);
            else
                sReturn = sInput;
            if (sReturn.Length > 99)
                sReturn = sReturn.Substring(0, 99);
            return sReturn;
        }

        /// <summary>
        /// 获取内容
        /// </summary>
        /// <param name="sOriContent">原始数据</param>
        /// <param name="sOtherRemoveReg">需要移除的字符</param>
        /// <param name="sPageUrl">URL</param>
        /// <param name="dtAntiLink">反链 表数据</param>
        /// <returns>转码后的内容</returns>
        public static string GetContent(string sOriContent, string sOtherRemoveReg, string sPageUrl, DataTable dtAntiLink)
        {
            string sFormartted = sOriContent;

            //去掉有危险的标记
            sFormartted = Regex.Replace(sFormartted, @"<script[\s\S]*?</script>", "", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
            sFormartted = Regex.Replace(sFormartted, @"<iframe[^>]*>[\s\S]*?</iframe>", "", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
            Regex r = new Regex(@"<input[\s\S]+?>|<form[\s\S]+?>|</form[\s\S]*?>|<select[\s\S]+?>?</select>|<textarea[\s\S]*?>?</textarea>|<file[\s\S]*?>|<noscript>|</noscript>", RegexOptions.IgnoreCase);
            sFormartted = r.Replace(sFormartted, "");
            string[] sOtherReg = sOtherRemoveReg.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string sRemoveReg in sOtherReg)
            {
                sFormartted = Replace(sFormartted, sRemoveReg, "", 0);
            }

            //图片路径
            sFormartted = _ReplaceUrl("<img[\\s\\S]+?src\\s*=\\s*(?:'(?<src>[^']+)'|\"(?<src>[^\"]+)\"|(?<src>[^>\\s]+))\\s*[^>]*>", "src", sFormartted, sPageUrl);
            //反防盗链
            string domain = GetDomain(sPageUrl);
            DataRow[] drs = dtAntiLink.Select("Domain='" + domain + "'");
            if (drs.Length > 0)
            {
                foreach (DataRow dr in drs)
                {
                    switch (Convert.ToInt32(dr["Type"]))
                    {
                        case 1://置换
                            sFormartted = sFormartted.Replace(dr["imgUrl"].ToString(), "http://stat.580k.com/t.asp?url=");
                            break;
                        default://附加
                            sFormartted = sFormartted.Replace(dr["imgUrl"].ToString(), "http://stat.580k.com/t.asp?url=" + dr["imgUrl"].ToString());
                            break;
                    }
                }
            }

            //A链接
            sFormartted = _ReplaceUrl(@"<a[^>]+href\s*=\s*(?:'(?<href>[^']+)'|""(?<href>[^""]+)""|(?<href>[^>\s]+))\s*[^>]*>", "href", sFormartted, sPageUrl);

            //CSS
            sFormartted = _ReplaceUrl(@"<link[^>]+href\s*=\s*(?:'(?<href>[^']+)'|""(?<href>[^""]+)""|(?<href>[^>\s]+))\s*[^>]*>", "href", sFormartted, sPageUrl);

            //BACKGROUND
            sFormartted = _ReplaceUrl(@"background\s*=\s*(?:'(?<img>[^']+)'|""(?<img>[^""]+)""|(?<img>[^>\s]+))", "img", sFormartted, sPageUrl);
            //style方式的背景：background-image:url(...)
            sFormartted = _ReplaceUrl(@"background-image\s*:\s*url\s*\x28(?<img>[^\x29]+)\x29", "img", sFormartted, sPageUrl);

            //FLASH
            sFormartted = _ReplaceUrl(@"<param\s[^>]+""movie""[^>]+value\s*=\s*""(?<flash>[^"">]+\x2eswf)""[^>]*>", "flash", sFormartted, sPageUrl);

            //XSL
            if (IsXml(sFormartted))
            {
                sFormartted = _ReplaceUrl(@"<\x3fxml-stylesheet\s+[^\x3f>]+href=\s*(?:'(?<href>[^']+)'|""(?<href>[^""]+)"")\s*[^\x3f>]*\x3f>", "href", sFormartted, sPageUrl);
            }

            //script
            //sFormartted = _ReplaceUrl(@"<script[^>]+src\s*=\s*(?:'(?<src>[^']+)'|""(?<src>[^""]+)""|(?<src>[^>\s]+))\s*[^>]*>", "src", sFormartted,sPageUrl);

            return sFormartted;
        }

        private static string _ReplaceUrl(string strRe, string subMatch, string sFormartted, string sPageUrl)
        {
            Regex re = new Regex(strRe, RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
            MatchCollection mcs = re.Matches(sFormartted);
            string sOriStr = "";
            string sSubMatch = "";
            string sReplaceStr = "";
            foreach (Match mc in mcs)
            {
                sOriStr = mc.Value;
                sSubMatch = mc.Groups[subMatch].Value;
                sReplaceStr = sOriStr.Replace(sSubMatch, GetUrl(sPageUrl, sSubMatch));
                sFormartted = sFormartted.Replace(sOriStr, sReplaceStr);
            }

            return sFormartted;
        }

        /// <summary>
        /// 判断是否是xml格式
        /// </summary>
        /// <param name="sFormartted">输入内容</param>
        /// <returns>是否是xml数据</returns>
        public static bool IsXml(string sFormartted)
        {
            Regex re = new Regex(@"<\x3fxml\s+", RegexOptions.IgnoreCase);
            MatchCollection mcs = re.Matches(sFormartted);
            return mcs.Count > 0;
        }

        #endregion 根据表达式，获得文章内容

        #region HTML相关操作
        /// <summary>
        /// 清除html标签
        /// </summary>
        /// <param name="sHtml">html代码</param>
        /// <returns>清理后的内容</returns>
        public static string ClearTag(string sHtml)
        {
            if (sHtml?.Length == 0)
                return "";
            string sTemp = sHtml;
            Regex re = new Regex(@"(<[^>\s]*\b(\w)+\b[^>]*>)|(<>)|(&nbsp;)|(&gt;)|(&lt;)|(&amp;)|\r|\n|\t", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
            return re.Replace(sHtml, "");
        }
        /// <summary>
        /// 根据正则清除html标签
        /// </summary>
        /// <param name="sHtml">html代码</param>
        /// <param name="sRegex">正则表达式</param>
        /// <returns>清理后的内容</returns>
        public static string ClearTag(string sHtml, string sRegex)
        {
            string sTemp = sHtml;
            Regex re = new Regex(sRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
            return re.Replace(sHtml, "");
        }
        /// <summary>
        /// 将html转换成js代码
        /// </summary>
        /// <param name="sHtml">html代码</param>
        /// <returns>js代码</returns>
        public static string ConvertToJavascript(string sHtml)
        {
            StringBuilder sText = new StringBuilder();
            var re = new Regex(@"\r\n", RegexOptions.IgnoreCase);
            string[] strArray = re.Split(sHtml);
            foreach (string strLine in strArray)
            {
                sText.Append("document.writeln(\"" + strLine.Replace("\"", "\\\"") + "\");\r\n");
            }
            return sText.ToString();
        }

        /// <summary>
        /// 删除字符串中的特定标记 
        /// </summary>
        /// <param name="str">html代码</param>
        /// <param name="tag">指定的标记</param>
        /// <param name="isContent">是否清除内容 </param>
        /// <returns>清理后的代码</returns>
        public static string DelTag(string str, string tag, bool isContent)
        {
            if (tag == null || tag == " ")
            {
                return str;
            }

            if (isContent) //要求清除内容 
            {
                return Regex.Replace(str, string.Format("<({0})[^>]*>([\\s\\S]*?)<\\/\\1>", tag), "", RegexOptions.IgnoreCase);
            }

            return Regex.Replace(str, string.Format(@"(<{0}[^>]*(>)?)|(</{0}[^>] *>)|", tag), "", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 删除字符串中的一组标记 
        /// </summary>
        /// <param name="str">html代码</param>
        /// <param name="tagA">标记</param>
        /// <param name="isContent">是否清除内容 </param>
        /// <returns>清理后的代码</returns>
        public static string DelTagArray(string str, string tagA, bool isContent)
        {
            string[] tagAa = tagA.Split(',');
            foreach (string sr1 in tagAa) //遍历所有标记，删除 
            {
                str = DelTag(str, sr1, isContent);
            }
            return str;
        }

        #endregion HTML相关操作

        #region 根据内容获得链接
        /// <summary>
        /// 根据内容获得链接
        /// </summary>
        /// <param name="sContent">html代码</param>
        /// <returns>链接</returns>
        public static string GetLink(string sContent)
        {
            string strReturn = "";
            Regex re = new Regex(@"<a\s+[^>]*href\s*=\s*(?:'(?<href>[^']+)'|""(?<href>[^""]+)""|(?<href>[^>\s]+))\s*[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
            Regex js = new Regex(@"(href|onclick)=[^>]+javascript[^>]+(('(?<href>[\w\d/-]+\.[^']*)')|(&quot;(?<href>[\w\d/-]+\.[^;]*)&quot;))[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
            Match mc = js.Match(sContent);//获取javascript中的链接，有待改进
            if (mc.Success)
            {
                strReturn = mc.Groups["href"].Value;
            }
            else
            {
                Match me = re.Match(sContent);
                if (me.Success)
                {
                    strReturn = System.Web.HttpUtility.HtmlDecode(me.Groups["href"].Value);
                    //strReturn = RemoveByReg(strReturn, @";.*|javascript:.*");
                    strReturn = RemoveByReg(strReturn, @";[^?&]*|javascript:.*");
                }
            }

            return strReturn;
        }
        /// <summary>
        /// 根据链接得到文本
        /// </summary>
        /// <param name="sContent">链接</param>
        /// <returns>文本</returns>
        public static string GetTextByLink(string sContent)
        {
            Regex re = new Regex(@"<a(?:\s+[^>]*)?>([\s\S]*)?</a>", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            Regex email = new Regex(@"(href|onclick)=[^>]+mailto[^>]+@[^>]+>", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            Match me = email.Match(sContent);
            if (me.Success)
                return "";

            Match mc = re.Match(sContent);
            if (mc.Success)
                return mc.Groups[1].Value;
            else
                return "";
        }

        private static void _GetLinks(string sContent, string sUrl, ref Dictionary<string, string> lisA)
        {
            const string sFilter =
@"首页|下载|中文|English|反馈|讨论区|投诉|建议|联系|关于|about|诚邀|工作|简介|新闻|掠影|风采
|登录|注销|注册|使用|体验|立即|收藏夹|收藏|添加|加入
|更多|more|专题|精选|热卖|热销|推荐|精彩
|加盟|联盟|友情|链接|相关
|订阅|阅读器|RSS
|免责|条款|声明|我的|我们|组织|概况|有限|免费|公司|法律|导航|广告|地图|隐私
|〖|〗|【|】|（|）|［|］|『|』|\.";

            Regex re = new Regex(@"<a\s+[^>]*href\s*=\s*[^>]+>[\s\S]*?</a>", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            Regex re2 = new Regex(@"""|'", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            MatchCollection mcs = re.Matches(sContent);
            //foreach (Match mc in mcs)
            for (int i = mcs.Count - 1; i >= 0; i--)
            {
                Match mc = mcs[i];
                string strHref = GetLink(mc.Value).Trim();

                strHref = strHref.Replace("\\\"", "");//针对JS输出链接
                strHref = strHref.Replace("\\\'", "");

                string strTemp = RemoveByReg(strHref, @"^http.*/$");//屏蔽以“http”开头“/”结尾的链接地址
                if (strTemp.Length < 2)
                {
                    continue;
                }

                //过滤广告或无意义的链接
                string strText = ClearTag(GetTextByLink(mc.Value)).Trim();
                strTemp = RemoveByReg(strText, sFilter);
                if (Encoding.Default.GetBytes(strTemp).Length < 9)
                {
                    continue;
                }
                if (re2.IsMatch(strText))
                {
                    continue;
                }

                //换上绝对地址
                strHref = GetUrlByRelative(sUrl, strHref);
                if (strHref.Length <= 18)//例如，http://www.163.com = 18
                {
                    continue;
                }

                //计算#字符出现的位置，移除它后面的内容
                //如果是域名地址，就跳过
                int charIndex = strHref.IndexOf('#');
                if (charIndex > -1)
                {
                    strHref = strHref.Substring(0, charIndex);
                }
                strHref = strHref.Trim(new char[] { '/', '\\' });
                string tmpDomainURL = GetDomain(strHref);
                if (strHref.Equals(tmpDomainURL, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!lisA.ContainsKey(strHref) && !lisA.ContainsValue(strText))
                {
                    lisA.Add(strHref, strText);
                }
            }
        }

        /// <summary>
        /// 判断是否是js链接
        /// </summary>
        /// <param name="sHtml">html</param>
        /// <returns>判断是否是js链接</returns>
        public static bool IsExistsScriptLink(string sHtml)
        {
            Regex re = new Regex(@"<script[^>]+src\s*=\s*(?:'(?<src>[^']+)'|""(?<src>[^""]+)""|(?<src>[^>\s]+))\s*[^>]*>", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
            return re.IsMatch(sHtml);
        }

        /// <summary>
        /// 从RSS FEED中读取
        /// </summary>
        /// <param name="sContent">RSS内容</param>
        /// <param name="sUrl">URL</param>
        /// <returns>读取到的内容</returns>
        public static Dictionary<string, string> GetLinksFromRss(string sContent, string sUrl)
        {
            Dictionary<string, string> lisDes = new Dictionary<string, string>();
            return GetLinksFromRss(sContent, sUrl, ref lisDes);
        }

        /// <summary>
        /// 从RSS FEED中读取
        /// </summary>
        /// <param name="sContent">RSS内容</param>
        /// <param name="sUrl">URL</param>
        /// <param name="lisDes">过滤条件</param>
        /// <returns>读取到的内容</returns>
        public static Dictionary<string, string> GetLinksFromRss(string sContent, string sUrl, ref Dictionary<string, string> lisDes)
        {
            Dictionary<string, string> listResult = new Dictionary<string, string>();

            XmlDocument xml = new XmlDocument();

            //RSS2.0
            try
            {
                xml.LoadXml(sContent.Trim());
                XmlNodeList nodes = xml.SelectNodes("/rss/channel/item");
                if (nodes.Count > 0)
                {
                    for (int i = nodes.Count - 1; i >= 0; i--)
                    {
                        try
                        {
                            string sLink = GetUrlByRelative(sUrl, nodes[i].SelectSingleNode("link").InnerText);
                            listResult.Add(sLink, nodes[i].SelectSingleNode("title").InnerText);
                            lisDes.Add(sLink, nodes[i].SelectSingleNode("description").InnerText);
                        }
                        catch (Exception e)
                        {
                            LogManager.Error(e);
                        }
                    }
                    return listResult;
                }
            }
            catch (Exception e)
            {
                LogManager.Error(e);
            }

            //RSS1.0（RDF）
            try
            {
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(xml.NameTable);
                nsMgr.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                nsMgr.AddNamespace("rss", "http://purl.org/rss/1.0/");
                XmlNodeList nodes = xml.SelectNodes("/rdf:RDF//rss:item", nsMgr);
                if (nodes.Count > 0)
                {
                    for (int i = nodes.Count - 1; i >= 0; i--)
                    {
                        try
                        {
                            string sLink = GetUrlByRelative(sUrl, nodes[i].SelectSingleNode("rss:link", nsMgr).InnerText);
                            listResult.Add(sLink, nodes[i].SelectSingleNode("rss:title", nsMgr).InnerText);
                            lisDes.Add(sLink, nodes[i].SelectSingleNode("rss:description", nsMgr).InnerText);
                        }
                        catch (Exception e)
                        {
                            LogManager.Error(e);
                        }
                        //listResult.Add("<a href=\"" + nodes[i].SelectSingleNode("rss:link",nsMgr).InnerText + "\">" + nodes[i].SelectSingleNode("rss:title",nsMgr).InnerText + "</a>");
                    }
                    return listResult;
                }
            }
            catch (Exception e)
            {
                LogManager.Error(e);
            }

            //RSS ATOM
            try
            {
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(xml.NameTable);
                nsMgr.AddNamespace("atom", "http://purl.org/atom/ns#");
                XmlNodeList nodes = xml.SelectNodes("/atom:feed/atom:entry", nsMgr);
                if (nodes.Count > 0)
                {
                    for (int i = nodes.Count - 1; i >= 0; i--)
                    {
                        try
                        {
                            string sLink = GetUrlByRelative(sUrl, nodes[i].SelectSingleNode("atom:link", nsMgr).Attributes["href"].InnerText);
                            listResult.Add(sLink, nodes[i].SelectSingleNode("atom:title", nsMgr).InnerText);
                            lisDes.Add(sLink, nodes[i].SelectSingleNode("atom:content", nsMgr).InnerText);
                        }
                        catch (Exception e)
                        {
                            LogManager.Error(e);
                        }
                        //listResult.Add("<a href=\"" + nodes[i].SelectSingleNode("atom:link",nsMgr).Attributes["href"].InnerText + "\">" + nodes[i].SelectSingleNode("atom:title",nsMgr).InnerText + "</a>");
                    }
                    return listResult;
                }
            }
            catch (Exception e)
            {
                LogManager.Error(e);
            }

            return listResult;
        }
        /// <summary>
        ///  从RSS FEED中读取标题
        /// </summary>
        /// <param name="sContent">RSS</param>
        /// <returns>标题</returns>
        public static string GetTitleFromRss(string sContent)
        {
            string title = "";
            XmlDocument xml = new XmlDocument();

            //RSS2.0
            try
            {
                xml.LoadXml(sContent.Trim());
                title = xml.SelectSingleNode("/rss/channel/title").InnerText;
            }
            catch (Exception e)
            {
                LogManager.Error(e);
            }

            return title;
        }

        /// <summary>
        /// 根据标签进行移除
        /// </summary>
        /// <param name="sContent">html</param>
        /// <param name="sRegex">正则表达式</param>
        /// <returns>清理后的代码</returns>
        public static string RemoveByReg(string sContent, string sRegex)
        {
            Regex re = new Regex(sRegex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            MatchCollection mcs = re.Matches(sContent);
            foreach (Match mc in mcs)
            {
                sContent = sContent.Replace(mc.Value, "");
            }
            return sContent;
        }

        /// <summary>
        /// 根据正则表达式替换内容
        /// </summary>
        /// <param name="sContent">html</param>
        /// <param name="sReplace">需要替换的内容</param>
        /// <param name="sRegex">符合正则的内容</param>
        /// <returns>替换后内容</returns>
        public static string ReplaceByReg(string sContent, string sReplace, string sRegex)
        {
            Regex re = new Regex(sRegex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            sContent = re.Replace(sContent, sReplace);
            return sContent;
        }

        /// <summary>
        ///  网页Body内容
        /// </summary>
        /// <param name="sContent">html源代码</param>
        /// <returns>网页Body内容</returns>
        public static string GetBody(string sContent)
        {
            Regex re = new Regex(@"[\s\S]*?<\bbody\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            sContent = re.Replace(sContent, "");

            re = new Regex(@"</\bbody\b[^>]*>\s*</html>", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.RightToLeft);
            sContent = re.Replace(sContent, "");
            return sContent;
        }
        #endregion 根据超链接地址获取页面内容

        #region 根据内容作字符串分析
        /// <summary>
        /// 根据标签获取文本
        /// </summary>
        /// <param name="sContent">html</param>
        /// <param name="sRegex">正则表达式</param>
        /// <returns>文本</returns>
        public static string GetTextByReg(string sContent, string sRegex)
        {
            Regex re = new Regex(sRegex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            Match mc = re.Match(sContent);
            string str = "";
            if (mc.Success)
                str = mc.Groups[0].Value;
            while (str.EndsWith("_"))
            {
                str = RemoveEndWith(str, "_");
            }
            return str;
        }

        // charset=[\s]*(?<Coding>[^'"]+)[\s]*['"]?[\s]*[/]?>
        /// <summary>
        /// 根据标签获取文本
        /// </summary>
        /// <param name="sContent">html</param>
        /// <param name="sRegex">正则表达式</param>
        /// <param name="sGroupName">分组名</param>
        /// <returns>文本</returns>
        public static string GetTextByReg(string sContent, string sRegex, string sGroupName)
        {
            Regex re = new Regex(sRegex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            Match mc = re.Match(sContent);
            string str = "";
            if (mc.Success)
                str = mc.Groups[sGroupName].Value;
            return str;
        }

        /// <summary>
        /// 获得链接的绝对路径
        /// </summary>
        /// <param name="sUrl">原链接地址</param>
        /// <param name="sRUrl">相对地址</param>
        /// <returns>获得链接的绝对路径</returns>
        public static string GetUrlByRelative(string sUrl, string sRUrl)
        {
            try
            {
                //http://q.yesky.com/grp/dsc/view.do;jsessionid=A6324FD46B4893303124F70C0B2AAC1E?grpId=201595&rvId=8215876
                Uri baseUri = new Uri(sUrl);
                if (!sUrl.EndsWith("/"))
                {
                    int i = baseUri.Segments.Length - 1;
                    if (i > 0)
                    {
                        string file = baseUri.Segments[i];
                        if (file.IndexOf('.') < 1)
                        {
                            baseUri = new Uri(sUrl + "/");
                        }
                    }
                }
                Uri myUri = new Uri(baseUri, sRUrl);
                return myUri.AbsoluteUri;
            }
            catch
            {
                return sUrl;
            }
        }

        /// <summary>
        /// 根据标签获取数据集合
        /// </summary>
        /// <param name="sContent">html</param>
        /// <param name="sRegex">正则表达式</param>
        /// <returns>数据集合</returns>
        public static List<string> GetListByReg(string sContent, string sRegex)
        {
            List<string> list = new List<string>();
            Regex re = new Regex(sRegex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            MatchCollection mcs = re.Matches(sContent);
            foreach (Match mc in mcs)
            {
                list.Add(mc.Groups["href"].Value);
            }
            return list;
        }

        /// <summary>
        /// 获得主域
        /// </summary>
        /// <param name="sUrl">URL</param>
        /// <returns>域名</returns>
        public static string GetDomainUrl(string sUrl)
        {
            try
            {
                Uri baseUri = new Uri(sUrl);

                return baseUri.Scheme + "://" + baseUri.Authority;
            }
            catch
            {
                return sUrl;
            }
        }

        #endregion

        #region 杂项

        /// <summary>
        /// 从html中过滤出文本
        /// </summary>
        /// <param name="sHtml">html</param>
        /// <returns>纯文本</returns>
        public static string GetTxtFromHtml(this string sHtml)
        {
            string del = @"<head[^>]*>[\s\S]*?</head>";
            string content = RemoveByReg(sHtml, del);

            del = @"(<script[^>]*>[\s\S]*?</script>)|(<IFRAME[^>]*>[\s\S]*?</IFRAME>)|(<style[^>]*>[\s\S]*?</style>|<title[^>]*>[\s\S]*?</title>|<meta[^>]*>|<option[^>]*>[\s\S]*?</option>)";
            content = RemoveByReg(content, del);

            del = @"(&nbsp;)|([\n\t]+)";
            content = RemoveByReg(content, del);

            string re = @"(<table(\s+[^>]*)*>)|(<td(\s+[^>]*)*>)|(<tr(\s+[^>]*)*>)|(<p(\s+[^>]*)*>)|(<div(\s+[^>]*)*>)|(<ul(\s+[^>]*)*>)|(<li(\s+[^>]*)*>)|</table>|</td>|</tr>|</p>|<br>|</div>|</li>|</ul>|<p />|<br />";
            content = ReplaceByReg(content, "", re);
            content = ReplaceByReg(content, "", @"[\f\n\r\v]+");

            content = RemoveByReg(content, @"<a(\s+[^>]*)*>[\s\S]*?</a>");
            content = RemoveByReg(content, "<[^>]+>");//去除各种HTML标记，获得纯内容

            content = content.Replace("\n", "");
            content = content.Replace("\r", "");
            content = content.Trim();
            return content;
        }

        /// <summary>
        /// 从html中过滤出文本，不过保留换行符号
        /// </summary>
        /// <param name="sHtml">html源代码</param>
        /// <returns>从html中过滤出文本，不过保留换行符号</returns>
        public static string GetTxtFromHtml2(this string sHtml)
        {
            string del = @"<head[^>]*>[\s\S]*?</head>";
            string content = RemoveByReg(sHtml, del);

            del = @"(<script[^>]*>[\s\S]*?</script>)|(<IFRAME[^>]*>[\s\S]*?</IFRAME>)|(<style[^>]*>[\s\S]*?</style>|<title[^>]*>[\s\S]*?</title>|<meta[^>]*>|<option[^>]*>[\s\S]*?</option>)";
            content = RemoveByReg(content, del);

            del = @"(&nbsp;)|([\t]+)";//del = @"(&nbsp;)|([\n\t]+)";
            content = RemoveByReg(content, del);

            string re = @"(<table(\s+[^>]*)*>)|(<td(\s+[^>]*)*>)|(<tr(\s+[^>]*)*>)|(<p(\s+[^>]*)*>)|(<div(\s+[^>]*)*>)|(<ul(\s+[^>]*)*>)|(<li(\s+[^>]*)*>)|</table>|</td>|</tr>|</p>|<br>|</div>|</li>|</ul>|<p />|<br />";
            content = ReplaceByReg(content, "", re);
            //content = CText.ReplaceByReg(content, "", @"[\f\n\r\v]+");

            content = RemoveByReg(content, @"<a(\s+[^>]*)*>[\s\S]*?</a>");
            content = RemoveByReg(content, "<[^>]+>");//去除各种HTML标记，获得纯内容
            content = content.Trim();

            return content;
        }
        #endregion

        /// <summary>
        /// 按结尾移除内容
        /// </summary>
        /// <param name="sOrg">原始数据</param>
        /// <param name="sEnd">结束的字符串</param>
        /// <returns>清理后的内容</returns>
        public static string RemoveEndWith(string sOrg, string sEnd)
        {
            if (sOrg.EndsWith(sEnd))
                sOrg = sOrg.Remove(sOrg.IndexOf(sEnd), sEnd.Length);
            return sOrg;
        }

        #region 根据超链接地址获取页面内容
        /// <summary>
        /// 根据超链接地址获取页面内容
        /// </summary>
        /// <param name="sUrl">URL</param>
        /// <returns>页面内容</returns>
        public static string GetHtmlByUrl(string sUrl)
        {
            return GetHtmlByUrl(sUrl, "auto");
        }

        /// <summary>
        /// 根据超链接地址获取页面内容
        /// </summary>
        /// <param name="sUrl">URL</param>
        /// <param name="sCoding">文件编码</param>
        /// <returns>页面内容</returns>
        public static string GetHtmlByUrl(string sUrl, string sCoding)
        {
            return GetHtmlByUrl(ref sUrl, sCoding);
        }

        /// <summary>
        /// 根据超链接地址获取页面内容，并将url作为引用类型
        /// </summary>
        /// <param name="sUrl">URL</param>
        /// <param name="sCoding">文件编码</param>
        /// <returns>页面内容</returns>
        public static string GetHtmlByUrl(ref string sUrl, string sCoding)
        {
            string content = "";

            try
            {
                HttpWebResponse response = _MyGetResponse(sUrl);
                if (response == null)
                {
                    return content;
                }

                sUrl = response.ResponseUri.AbsoluteUri;

                Stream stream = response.GetResponseStream();
                byte[] buffer = GetContent(stream);
                stream.Close();
                stream.Dispose();

                string charset = "";
                if (string.IsNullOrEmpty(sCoding) || string.Equals(sCoding, "auto", StringComparison.CurrentCultureIgnoreCase))
                {//如果不指定编码，那么系统代为指定
                    //首先，从返回头信息中寻找
                    string ht = response.GetResponseHeader("Content-Type");
                    response.Close();
                    string regCharSet = "[\\s\\S]*charset=(?<charset>[\\S]*)";
                    Regex r = new Regex(regCharSet, RegexOptions.IgnoreCase);
                    Match m = r.Match(ht);
                    charset = (m.Captures.Count != 0) ? m.Result("${charset}") : "";
                    if (charset == "-8") charset = "utf-8";
                    if (charset?.Length == 0)
                    {//找不到，则在文件信息本身中查找
                        //先按gb2312来获取文件信息
                        content = System.Text.Encoding.GetEncoding("gb2312").GetString(buffer);

                        regCharSet = "(<meta[^>]*charset=(?<charset>[^>'\"]*)[\\s\\S]*?>)|(xml[^>]+encoding=(\"|')*(?<charset>[^>'\"]*)[\\s\\S]*?>)";
                        r = new Regex(regCharSet, RegexOptions.IgnoreCase);
                        m = r.Match(content);
                        if (m.Captures.Count == 0)
                        {//没办法，都找不到编码，只能返回按"gb2312"获取的信息
                            //content = CText.RemoveByReg(content, @"<!--[\s\S]*?-->");
                            return content;
                        }
                        charset = m.Result("${charset}");
                    }
                }
                else
                {
                    response.Close();
                    charset = sCoding.ToLower();
                }

                try
                {
                    content = System.Text.Encoding.GetEncoding(charset).GetString(buffer);
                }
                catch (ArgumentException)
                {//指定的编码不可识别
                    content = System.Text.Encoding.GetEncoding("gb2312").GetString(buffer);
                }

                //content = CText.RemoveByReg(content, @"<!--[\s\S]*?-->");
            }
            catch
            {
                content = "";
            }

            return content;
        }

        private static HttpWebResponse _MyGetResponse(string sUrl)
        {
            int iTimeOut = 10000;
            //try
            //{
            //    //iTimeOut = int.Parse(System.Configuration.ConfigurationManager.AppSettings["SocketTimeOut"]);
            //}
            //catch { iTimeOut = 10000; }

            bool bCookie = false;
            bool bRepeat = false;
            Uri target = new Uri(sUrl);

ReCatch:
            try
            {
                HttpWebRequest resquest = (HttpWebRequest)WebRequest.Create(target);
                resquest.MaximumResponseHeadersLength = -1;
                resquest.ReadWriteTimeout = 120000;//120秒就超时
                resquest.Timeout = iTimeOut;
                resquest.MaximumAutomaticRedirections = 50;
                resquest.MaximumResponseHeadersLength = 5;
                resquest.AllowAutoRedirect = true;
                if (bCookie)
                {
                    resquest.CookieContainer = new CookieContainer();
                }
                resquest.UserAgent = "Mozilla/6.0 (compatible; MSIE 6.0; Windows NT 5.1)";
                //resquest.UserAgent = @"Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727; InfoPath.1) Web-Sniffer/1.0.24";
                //resquest.KeepAlive = true;
                return (HttpWebResponse)resquest.GetResponse();
            }
            catch (WebException)
            {
                if (!bRepeat)
                {
                    bRepeat = true;
                    bCookie = true;
                    goto ReCatch;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private static byte[] GetContent(Stream stream)
        {
            ArrayList arBuffer = new ArrayList();
            const int BUFFSIZE = 4096;

            try
            {
                byte[] buffer = new byte[BUFFSIZE];
                int count = stream.Read(buffer, 0, BUFFSIZE);
                while (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        arBuffer.Add(buffer[i]);
                    }
                    count = stream.Read(buffer, 0, BUFFSIZE);
                }
            }
            catch (Exception e)
            {
                LogManager.Error(e);
            }

            return (byte[])arBuffer.ToArray(System.Type.GetType("System.Byte"));
        }

        /// <summary>
        /// 获取http报文头
        /// </summary>
        /// <param name="sUrl">URL</param>
        /// <returns>报文信息</returns>
        public static string GetHttpHead(string sUrl)
        {
            string sHead = "";
            Uri uri = new Uri(sUrl);
            try
            {
                WebRequest req = WebRequest.Create(uri);
                WebResponse resp = req.GetResponse();
                WebHeaderCollection headers = resp.Headers;
                string[] sKeys = headers.AllKeys;
                foreach (string sKey in sKeys)
                {
                    sHead += sKey + ":" + headers[sKey] + "\r\n";
                }
            }
            catch (Exception e)
            {
                LogManager.Error(e);
            }
            return sHead;
        }

        /// <summary>
        /// 处理框架页面问题。如果该页面是框架结构的话，返回该框架
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="content">内容</param>
        /// <returns>框架结构</returns>
        public static string[] DealWithFrame(string url, string content)
        {
            string regFrame = @"<frame\s+[^>]*src\s*=\s*(?:""(?<src>[^""]+)""|'(?<src>[^']+)'|(?<src>[^\s>""']+))[^>]*>";
            return DealWithFrame(regFrame, url, content);
        }

        /// <summary>
        /// 处理浮动桢问题。如果该页面存在浮动桢，返回浮动桢
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="content">内容</param>
        /// <returns>浮动桢</returns>
        public static string[] DealWithIFrame(string url, string content)
        {
            string regiFrame = @"<iframe\s+[^>]*src\s*=\s*(?:""(?<src>[^""]+)""|'(?<src>[^']+)'|(?<src>[^\s>""']+))[^>]*>";
            return DealWithFrame(regiFrame, url, content);
        }

        private static string[] DealWithFrame(string strReg, string url, string content)
        {
            ArrayList alFrame = new ArrayList();
            Regex r = new Regex(strReg, RegexOptions.IgnoreCase);
            Match m = r.Match(content);
            while (m.Success)
            {
                alFrame.Add(GetUrl(url, m.Groups["src"].Value));
                m = m.NextMatch();
            }

            return (string[])alFrame.ToArray(System.Type.GetType("System.String"));
        }

        #endregion 根据超链接地址获取页面内容

        #region 获得多个页面
        /// <summary>
        /// 获得多个页面
        /// </summary>
        /// <param name="listUrl">URL集合</param>
        /// <param name="sCoding">文件编码</param>
        /// <returns>页面集合</returns>
        /// <exception cref="Exception"> </exception>
        public static List<KeyValuePair<int, string>> GetHtmlByUrlList(List<KeyValuePair<int, string>> listUrl, string sCoding)
        {
            int iTimeOut = 120000;
            StringBuilder sbHtml = new StringBuilder();
            List<KeyValuePair<int, string>> listResult = new List<KeyValuePair<int, string>>();
            Socket sock = null;
            try
            {
                // 初始化				
                Uri site = new Uri(listUrl[0].Value);
                var ipHostInfo = Dns.GetHostEntry(site.Host);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, site.Port);
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { SendTimeout = iTimeOut, ReceiveTimeout = iTimeOut };
                sock.Connect(remoteEP);
                foreach (KeyValuePair<int, string> kvUrl in listUrl)
                {
                    site = new Uri(kvUrl.Value);
                    string sendMsg = "GET " + HttpUtility.UrlDecode(site.PathAndQuery) + " HTTP/1.1\r\n" +
                        "Accept: image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/vnd.ms-excel, application/msword, application/vnd.ms-powerpoint, */*\r\n" +
                        "Accept-Language:en-us\r\n" +
                        "Accept-Encoding:gb2312, deflate\r\n" +
                        "User-Agent: Mozilla/4.0\r\n" +
                        "Host: " + site.Host + "\r\n\r\n" + '\0';
                    // 发送
                    byte[] msg = Encoding.GetEncoding(sCoding).GetBytes(sendMsg);
                    int nBytes;
                    if ((nBytes = sock.Send(msg)) == 0)
                    {
                        sock.Shutdown(SocketShutdown.Both);
                        sock.Close();
                        return listResult;
                    }
                    // 接受
                    byte[] bytes = new byte[2048];
                    byte bt = Convert.ToByte('\x7f');
                    do
                    {
                        int count = 0;
                        try
                        {
                            nBytes = sock.Receive(bytes, bytes.Length - 1, 0);
                        }
                        catch (Exception Ex)
                        {
                            string str = Ex.Message;
                            nBytes = -1;
                        }
                        if (nBytes <= 0) break;
                        if (bytes[nBytes - 1] > bt)
                        {
                            for (int i = nBytes - 1; i >= 0; i--)
                            {
                                if (bytes[i] > bt)
                                    count++;
                                else
                                    break;
                            }
                            if (count % 2 == 1)
                            {
                                count = sock.Receive(bytes, nBytes, 1, 0);
                                if (count < 0)
                                    break;
                                nBytes += count;
                            }
                        }
                        else
                        {
                            bytes[nBytes] = (byte)'\0';
                        }

                        string s = Encoding.GetEncoding(sCoding).GetString(bytes, 0, nBytes);
                        sbHtml.Append(s);
                    } while (nBytes > 0);

                    listResult.Add(new KeyValuePair<int, string>(kvUrl.Key, sbHtml.ToString()));
                    sbHtml = null;
                    sbHtml = new StringBuilder();
                }
            }
            catch (Exception Ex)
            {
                string s = Ex.Message;
                try
                {
                    sock.Shutdown(SocketShutdown.Both);
                    sock.Close();
                }
                catch (Exception e)
                {
                    LogManager.Error(e);
                }
            }
            finally
            {
                try
                {
                    sock.Shutdown(SocketShutdown.Both);
                    sock.Close();
                }
                catch (Exception e)
                {
                    LogManager.Error(e);
                }
            }
            return listResult;
        }
        #endregion 根据超链接地址获取页面内容

        /// <summary>
        /// 页面类型枚举
        /// </summary>
        public enum PageType : int
        {
            /// <summary>
            /// HTML格式
            /// </summary>
            HTML = 0,
            /// <summary>
            /// RSS格式
            /// </summary>
            RSS = 1
        }
        /// <summary>
        /// 获取页面类型
        /// </summary>
        /// <param name="sUrl">URL</param>
        /// <param name="sHtml">内容</param>
        /// <returns>页面类型枚举</returns>
        public static PageType GetPageType(string sUrl, ref string sHtml)
        {
            PageType pt = PageType.HTML;

            //看有没有RSS FEED
            string regRss = @"<link\s+[^>]*((type=""application/rss\+xml"")|(type=application/rss\+xml))[^>]*>";
            Regex r = new Regex(regRss, RegexOptions.IgnoreCase);
            Match m = r.Match(sHtml);
            if (m.Captures.Count != 0)
            {//有，则转向从RSS FEED中抓取
                string regHref = @"href=\s*(?:'(?<href>[^']+)'|""(?<href>[^""]+)""|(?<href>[^>\s]+))";
                r = new Regex(regHref, RegexOptions.IgnoreCase);
                m = r.Match(m.Captures[0].Value);
                if (m.Captures.Count > 0)
                {
                    //有可能是相对路径，加上绝对路径
                    string rssFile = GetUrl(sUrl, m.Groups["href"].Value);
                    sHtml = GetHtmlByUrl(rssFile);
                    pt = PageType.RSS;
                }
            }
            else
            {//看这个地址本身是不是一个Rss feed
                r = new Regex(@"<rss\s+[^>]*>", RegexOptions.IgnoreCase);
                m = r.Match(sHtml);
                if (m.Captures.Count > 0)
                {
                    pt = PageType.RSS;
                }
            }

            return pt;
        }
    }
}
