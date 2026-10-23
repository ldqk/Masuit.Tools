using AngleSharp;
using AngleSharp.Dom;
using Ganss.Xss;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Masuit.Tools.Html;

/// <summary>
/// html工具类
/// </summary>
public static class HtmlTools
{
    /// <param name="html"></param>
    extension(string html)
    {
        /// <summary>
        /// 标准的防止html的xss净化器
        /// </summary>
        /// <returns></returns>
        public string HtmlSanitizerStandard()
        {
            var sanitizer = new HtmlSanitizer
            {
                KeepChildNodes = true
            };
            sanitizer.AllowedAttributes.Remove("id");
            sanitizer.AllowedAttributes.Remove("alt");
            sanitizer.AllowedCssProperties.Remove("font-family");
            sanitizer.AllowedTags.Remove("input");
            sanitizer.AllowedTags.Remove("button");
            sanitizer.AllowedTags.Remove("iframe");
            sanitizer.AllowedTags.Remove("frame");
            sanitizer.AllowedTags.Remove("textarea");
            sanitizer.AllowedTags.Remove("select");
            sanitizer.AllowedTags.Remove("form");
            sanitizer.AllowedAttributes.Add("src");
            sanitizer.AllowedAttributes.Add("class");
            sanitizer.AllowedAttributes.Add("style");
            return sanitizer.Sanitize(html);
        }

        /// <summary>
        /// 自定义的防止html的xss净化器
        /// </summary>
        /// <param name="labels">需要移除的标签集合</param>
        /// <param name="attributes">需要移除的属性集合</param>
        /// <param name="styles">需要移除的样式集合</param>
        /// <returns></returns>
        public string HtmlSanitizerCustom(string[] labels = null, string[] attributes = null, string[] styles = null)
        {
            var sanitizer = new HtmlSanitizer
            {
                KeepChildNodes = true
            };
            sanitizer.AllowedAttributes.Remove("id");
            sanitizer.AllowedAttributes.Remove("alt");
            sanitizer.AllowedCssProperties.Remove("font-family");
            sanitizer.AllowedTags.Remove("input");
            sanitizer.AllowedTags.Remove("button");
            sanitizer.AllowedTags.Remove("iframe");
            sanitizer.AllowedTags.Remove("frame");
            sanitizer.AllowedTags.Remove("textarea");
            sanitizer.AllowedTags.Remove("select");
            sanitizer.AllowedTags.Remove("form");
            sanitizer.AllowedAttributes.Add("src");
            sanitizer.AllowedAttributes.Add("class");
            sanitizer.AllowedAttributes.Add("style");
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
        /// <param name="length">截取长度</param>
        /// <returns></returns>
        public string RemoveHtmlTag(int length = 0)
        {
            var context = BrowsingContext.New(Configuration.Default);
            var doc = context.OpenAsync(req => req.Content(html)).Result;
            var strText = HttpUtility.HtmlEncode(doc.Body.TextContent);
            if (length > 0 && strText.Length > length)
            {
                return strText[..length];
            }

            return strText;
        }

        /// <summary>
        /// 替换html的img路径为绝对路径
        /// </summary>
        /// <param name="imgDest"></param>
        /// <returns></returns>
        public string ReplaceHtmlImgSource(string imgDest) => html.Replace("<img src=\"", "<img src=\"" + imgDest + "/");

        /// <summary>
        /// 将src的绝对路径换成相对路径
        /// </summary>
        /// <returns></returns>
        public string ConvertImgSrcToRelativePath()
        {
            return Regex.Replace(html, @"<img src=""(http:\/\/.+?)/", @"<img src=""/");
        }

        /// <summary>
        /// 匹配html的所有img标签集合
        /// </summary>
        /// <returns></returns>
        public IHtmlCollection<IElement> MatchImgTags()
        {
            var context = BrowsingContext.New(Configuration.Default);
            var doc = context.OpenAsync(req => req.Content(html)).Result;
            return doc.Body.GetElementsByTagName("img");
        }

        /// <summary>
        /// 匹配html的所有img标签的src集合
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> MatchImgSrcs()
        {
            return MatchImgTags(html).Where(n => n.HasAttribute("src")).Select(n => n.GetAttribute("src"));
        }

        /// <summary>
        /// 获取html中第一个img标签的src
        /// </summary>
        /// <returns></returns>
        public string MatchFirstImgSrc()
        {
            return MatchImgSrcs(html).FirstOrDefault();
        }

        /// <summary>
        /// 随机获取html代码中的img标签的src属性
        /// </summary>
        /// <returns></returns>
        public string MatchRandomImgSrc()
        {
            return MatchImgSrcs(html).OrderByRandom().FirstOrDefault();
        }
    }
}