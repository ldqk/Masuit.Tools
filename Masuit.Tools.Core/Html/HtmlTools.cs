using Ganss.XSS;
using Masuit.Tools.Win32;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Masuit.Tools.Html
{
    /// <summary>
    /// html工具类
    /// </summary>
    public static partial class HtmlTools
    {
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
        /// <summary>
        /// 去除html标签后并截取字符串
        /// </summary>
        /// <param name="html">源html</param>
        /// <param name="length">截取长度</param>
        /// <returns></returns>
        public static string RemoveHtmlTag(this string html, int length = 0)
        {
            string strText = Regex.Replace(html, "<[^>]+>", "");
            strText = Regex.Replace(strText, "&[^;]+;", "");
            if (length > 0 && strText.Length > length)
            {
                return strText.Substring(0, length);
            }

            return strText;
        }

        /// <summary>
        /// 清理Word文档转html后的冗余标签属性
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string ClearHtml(this string html)
        {
            string s = Regex.Match(Regex.Replace(html, @"background-color:#?\w{3,7}|font-family:'?[\w|\(|\)]*'?;?", string.Empty), @"<body[^>]*>([\s\S]*)<\/body>").Groups[1].Value.Replace("&#xa0;", string.Empty);
            s = Regex.Replace(s, @"\w+-?\w+:0\w+;?", string.Empty); //去除多余的零值属性
            s = Regex.Replace(s, "alt=\"(.+?)\"", string.Empty); //除去alt属性
            s = Regex.Replace(s, @"-aw.+?\s", string.Empty); //去除Word产生的-aw属性
            return s;
        }

        ///<summary>   
        /// 清除HTML标记   
        ///</summary>   
        ///<param name="htmlstring">包括HTML的源码</param>   
        ///<returns>已经去除后的文字</returns>   
        public static string RemoveHtml(this string htmlstring)
        {
            //删除脚本   
            htmlstring = Regex.Replace(htmlstring, "<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);

            //删除HTML   
            Regex regex = new Regex("<.+?>", RegexOptions.IgnoreCase);
            htmlstring = regex.Replace(htmlstring, "");
            htmlstring = Regex.Replace(htmlstring, "<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            htmlstring = Regex.Replace(htmlstring, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
            htmlstring = Regex.Replace(htmlstring, "-->", "", RegexOptions.IgnoreCase);
            htmlstring = Regex.Replace(htmlstring, "<!--.*", "", RegexOptions.IgnoreCase);
            htmlstring = Regex.Replace(htmlstring, "&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            htmlstring = Regex.Replace(htmlstring, "&(amp|#38);", "&", RegexOptions.IgnoreCase);
            htmlstring = Regex.Replace(htmlstring, "&(lt|#60);", "<", RegexOptions.IgnoreCase);
            htmlstring = Regex.Replace(htmlstring, "&(gt|#62);", ">", RegexOptions.IgnoreCase);
            htmlstring = Regex.Replace(htmlstring, "&(nbsp|#160);", "   ", RegexOptions.IgnoreCase);
            htmlstring = Regex.Replace(htmlstring, "&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            htmlstring = Regex.Replace(htmlstring, "&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            htmlstring = Regex.Replace(htmlstring, "&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            htmlstring = Regex.Replace(htmlstring, "&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            htmlstring = Regex.Replace(htmlstring, @"&#(\d+);", "", RegexOptions.IgnoreCase);

            htmlstring.Replace("<", "");
            htmlstring.Replace(">", "");
            htmlstring.Replace("\r\n", "");

            return htmlstring;
        }

        /// <summary>
        /// 替换html的img路径为绝对路径
        /// </summary>
        /// <param name="html"></param>
        /// <param name="imgDest"></param>
        /// <returns></returns>
        public static string ReplaceHtmlImgSource(this string html, string imgDest) => html.Replace("<img src=\"", "<img src=\"" + imgDest + "/");

        /// <summary>
        /// 将src的绝对路径换成相对路径
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ConvertImgSrcToRelativePath(this string s)
        {
            return Regex.Replace(s, @"<img src=""(http:\/\/.+?)/", @"<img src=""/");
        }

        private static readonly Regex ImgRegex = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<src>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>");

        /// <summary>
        /// 匹配html的所有img标签集合
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static MatchCollection MatchImgTags(this string html) => ImgRegex.Matches(html);

        /// <summary>
        /// 匹配html的所有img标签的src集合
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static IEnumerable<string> MatchImgSrcs(this string html)
        {
            foreach (Match m in ImgRegex.Matches(html))
            {
                yield return m.Groups["src"].Value;
            }
        }

        /// <summary>
        /// 匹配html的一个img标签
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static Match MatchImgTag(this string html) => ImgRegex.Match(html);

        /// <summary>
        /// 获取html中第一个img标签的src
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string MatchFirstImgSrc(this string html)
        {
            string src = ImgRegex.Match(html).Groups["src"].Value;
            int index = src.IndexOf("\"", StringComparison.Ordinal);
            if (index > 0)
            {
                src = src.Substring(0, index);
            }

            return src;
        }

        /// <summary>
        /// 随机获取html代码中的img标签的src属性
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string MatchRandomImgSrc(this string html)
        {
            var collection = ImgRegex.Matches(html);
            if (collection.Count > 0)
            {
                string src = collection[new Random().StrictNext(collection.Count)].Groups["src"].Value;
                int index = src.IndexOf("\"", StringComparison.Ordinal);
                if (index > 0)
                {
                    src = src.Substring(0, index);
                }

                return src;
            }

            return string.Empty;
        }

        /// <summary>
        /// 替换回车换行符为html换行符
        /// </summary>
        /// <param name="str">html</param>
        public static string StrFormat(this string str)
        {
            str = str.Replace("\r\n", "<br />");
            str = str.Replace("\n", "<br />");
            var str2 = str;

            return str2;
        }

        /// <summary>
        /// 替换html字符
        /// </summary>
        /// <param name="strHtml">html</param>
        public static string EncodeHtml(this string strHtml)
        {
            if (strHtml != "")
            {
                strHtml = strHtml.Replace(",", "&def");
                strHtml = strHtml.Replace("'", "&dot");
                strHtml = strHtml.Replace(";", "&dec");
                return strHtml;
            }

            return "";
        }
    }
}