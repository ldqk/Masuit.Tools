using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Masuit.Tools.Win32;

namespace Masuit.Tools.Html
{
    /// <summary>
    ///1、获取HTML<br/>
    ///1.1获取指定页面的HTML代码 GetHtml(string url, string postData, bool isPost, CookieContainer cookieContainer)<br/>
    ///1.2获取HTMLGetHtml(string url, CookieContainer cookieContainer)<br/>
    ///2、获取字符流<br/>
    ///2.1获取字符流GetStream(string url, CookieContainer cookieContainer)<br/>
    ///3、清除HTML标记 <br/>
    ///3.1清除HTML标记  NoHTML(string Htmlstring)<br/>
    ///4、匹配页面的链接 <br/>
    ///4.1获取页面的链接正则 GetHref(string HtmlCode)<br/>
    ///5、匹配页面的图片地址<br/>
    /// 5.1匹配页面的图片地址 GetImgSrc(string HtmlCode, string imgHttp)<br/>
    ///5.2匹配<img src="" />中的图片路径实际链接  GetImg(string ImgString, string imgHttp)<br/>
    ///6、抓取远程页面内容<br/>
    /// 6.1以GET方式抓取远程页面内容 Get_Http(string tUrl)<br/>
    /// 6.2以POST方式抓取远程页面内容 Post_Http(string url, string postData, string encodeType)<br/>
    ///7、压缩HTML输出<br/>
    ///7.1压缩HTML输出 ZipHtml(string Html)<br/>
    ///8、过滤HTML标签<br/>
    /// 8.1过滤指定HTML标签 DelHtml(string s_TextStr, string html_Str)  <br/>
    /// 8.2过滤HTML中的不安全标签 RemoveUnsafeHtml(string content)<br/>
    /// HTML转行成TEXT HtmlToTxt(string strHtml)<br/>
    /// 字符串转换为 HtmlStringToHtml(string str)<br/>
    /// html转换成字符串HtmlToString(string strHtml)<br/>
    /// 获取URL编码<br/>
    /// 判断URL是否有效<br/>
    /// 返回 HTML 字符串的编码解码结果
    /// </summary>
    public static partial class HtmlTools
    {
        #region 私有字段
        private static CookieContainer cc = new CookieContainer();
        private static string contentType = "application/x-www-form-urlencoded";
        private static string accept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg," +
                                       " application/x-shockwave-flash, application/x-silverlight, " +
                                       "application/vnd.ms-excel, application/vnd.ms-powerpoint, " +
                                       "application/msword, application/x-ms-application," +
                                       " application/x-ms-xbap," +
                                       " application/vnd.ms-xpsdocument, application/xaml+xml, application/x-silverlight-2-b1, */*";
        private static string userAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1;" +
                                          " .NET CLR 2.0.50727; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022)";
        private static int delay = 1000;
        private static int currentTry = 0;
        #endregion

        #region 公有属性
        /// <summary> 
        /// Cookie容器
        /// </summary> 
        public static CookieContainer CookieContainer
        {
            get { return cc; }
        }

        /// <summary> 
        /// 获取网页源码时使用的编码
        /// </summary> 
        public static Encoding Encoding { get; set; } = Encoding.GetEncoding("utf-8");

        /// <summary>
        /// 网络延迟
        /// </summary>
        public static int NetworkDelay
        {
            get
            {
                Random r = new Random();
                return r.Next(delay, delay * 2);
                // return (r.Next(delay / 1000, delay / 1000 * 2)) * 1000;
            }
            set { delay = value; }
        }

        /// <summary>
        /// 最大尝试次数
        /// </summary>
        public static int MaxTry { get; set; } = 300;
        #endregion

        #region 1、获取HTML

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
        /// 获取指定页面的HTML代码
        /// </summary>
        /// <param name="_"></param>
        /// <param name="url">指定页面的路径</param>
        /// <param name="postData">post 提交的字符串</param>
        /// <param name="isPost">是否以post方式发送请求</param>
        /// <param name="cookieContainer">Cookie集合</param>
        public static string GetHtml(this HttpWebRequest _, string url, string postData, bool isPost, CookieContainer cookieContainer)
        {
            if (string.IsNullOrEmpty(postData))
            {
                return GetHtml(null, url, cookieContainer);
            }
            Thread.Sleep(NetworkDelay);
            currentTry++;
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            try
            {
                byte[] byteRequest = Encoding.Default.GetBytes(postData);

                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.CookieContainer = cookieContainer;
                httpWebRequest.ContentType = contentType;
                httpWebRequest.ServicePoint.ConnectionLimit = MaxTry;
                httpWebRequest.Referer = url;
                httpWebRequest.Accept = accept;
                httpWebRequest.UserAgent = userAgent;
                httpWebRequest.Method = isPost ? "POST" : "GET";
                httpWebRequest.ContentLength = byteRequest.Length;

                httpWebRequest.AllowAutoRedirect = false;

                Stream stream = httpWebRequest.GetRequestStream();
                stream.Write(byteRequest, 0, byteRequest.Length);
                stream.Close();

                try
                {
                    httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    //redirectURL = httpWebResponse.Headers["Location"];// Get redirected uri
                }
                catch (WebException ex)
                {
                    httpWebResponse = (HttpWebResponse)ex.Response;
                }
                //httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, Encoding);
                string html = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();
                currentTry = 0;
                httpWebRequest.Abort();
                httpWebResponse.Close();
                return html;
            }
            catch (Exception)
            {
                if (currentTry <= MaxTry)
                {
                    GetHtml(null, url, postData, isPost, cookieContainer);
                }
                currentTry--;
                if (httpWebRequest != null) httpWebRequest.Abort();
                if (httpWebResponse != null) httpWebResponse.Close();
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取HTML
        /// </summary>
        /// <param name="_"></param>
        /// <param name="url">地址</param>
        /// <param name="cookieContainer">Cookie集合</param>
        public static string GetHtml(this HttpWebRequest _, string url, CookieContainer cookieContainer)
        {
            Thread.Sleep(NetworkDelay);
            currentTry++;
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.CookieContainer = cookieContainer;
                httpWebRequest.ContentType = contentType;
                httpWebRequest.ServicePoint.ConnectionLimit = MaxTry;
                httpWebRequest.Referer = url;
                httpWebRequest.Accept = accept;
                httpWebRequest.UserAgent = userAgent;
                httpWebRequest.Method = "GET";
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, Encoding);
                string html = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();
                currentTry--;
                httpWebRequest.Abort();
                httpWebResponse.Close();
                return html;
            }
            catch (Exception)
            {
                if (currentTry <= MaxTry) GetHtml(null, url, cookieContainer);
                currentTry--;
                if (httpWebRequest != null) httpWebRequest.Abort();
                if (httpWebResponse != null) httpWebResponse.Close();
                return string.Empty;
            }
        }
        #endregion

        #region 2、获取字符流

        ///  <summary>
        ///  2.1获取字符流
        ///  </summary>
        /// ---------------------------------------------------------------------------------------------------------------
        ///  示例:
        ///  System.Net.CookieContainer cookie = new System.Net.CookieContainer(); 
        ///  Stream s = HttpHelper.GetStream("http://www.baidu.com", cookie);
        ///  picVerify.Image = Image.FromStream(s);
        /// ---------------------------------------------------------------------------------------------------------------
        /// <param name="_"></param>
        /// <param name="url">地址</param>
        ///  <param name="cookieContainer">cookieContainer</param>
        public static Stream GetStream(this HttpWebRequest _, string url, CookieContainer cookieContainer)
        {
            currentTry++;

            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;

            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.CookieContainer = cookieContainer;
                httpWebRequest.ContentType = contentType;
                httpWebRequest.ServicePoint.ConnectionLimit = MaxTry;
                httpWebRequest.Referer = url;
                httpWebRequest.Accept = accept;
                httpWebRequest.UserAgent = userAgent;
                httpWebRequest.Method = "GET";

                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                currentTry--;
                return responseStream;
            }
            catch (Exception)
            {
                if (currentTry <= MaxTry)
                {
                    GetHtml(null, url, cookieContainer);
                }

                currentTry--;

                if (httpWebRequest != null)
                {
                    httpWebRequest.Abort();
                }
                if (httpWebResponse != null)
                {
                    httpWebResponse.Close();
                }
                return null;
            }
        }
        #endregion

        #region 3、清除HTML标记

        /// <summary>
        /// 清理Word文档转html后的冗余标签属性
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string ClearHtml(this string html)
        {
            string s = Regex.Match(Regex.Replace(html, @"background-color:#?\w{3,7}|font-family:'?[\w|\(|\)]*'?;?", string.Empty), @"<body[^>]*>([\s\S]*)<\/body>").Groups[1].Value.Replace("&#xa0;", string.Empty);
            s = Regex.Replace(s, @"\w+-?\w+:0\w+;?", string.Empty);//去除多余的零值属性
            s = Regex.Replace(s, "alt=\"(.+?)\"", string.Empty);//除去alt属性
            s = Regex.Replace(s, @"-aw.+?\s", string.Empty);//去除Word产生的-aw属性
            return s;
        }

        ///<summary>   
        ///3.1清除HTML标记   
        ///</summary>   
        ///<param name="Htmlstring">包括HTML的源码</param>   
        ///<returns>已经去除后的文字</returns>   
        public static string RemoveHTML(this string Htmlstring)
        {
            //删除脚本   
            Htmlstring = Regex.Replace(Htmlstring, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);

            //删除HTML   
            Regex regex = new Regex("<.+?>", RegexOptions.IgnoreCase);
            Htmlstring = regex.Replace(Htmlstring, "");
            Htmlstring = Regex.Replace(Htmlstring, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"-->", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<!--.*", "", RegexOptions.IgnoreCase);

            Htmlstring = Regex.Replace(Htmlstring, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(nbsp|#160);", "   ", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&#(\d+);", "", RegexOptions.IgnoreCase);

            Htmlstring.Replace("<", "");
            Htmlstring.Replace(">", "");
            Htmlstring.Replace("\r\n", "");

            return Htmlstring;
        }

        #endregion

        #region 4、匹配页面的链接
        #region 4.1获取页面的链接正则
        /// <summary>
        /// 4.1获取页面的链接正则
        /// </summary>
        /// <param name="HtmlCode">html代码</param>
        public static string GetHref(this string HtmlCode)
        {
            string MatchVale = "";
            string Reg = @"(h|H)(r|R)(e|E)(f|F) *= *('|"")?((\w|\\|\/|\.|:|-|_)+)[\S]*";
            foreach (Match m in Regex.Matches(HtmlCode, Reg))
            {
                MatchVale += (m.Value).ToLower().Replace("href=", "").Trim() + "|";
            }
            return MatchVale;
        }
        #endregion

        #region  4.2取得所有链接URL
        /// <summary>
        /// 4.2取得所有链接URL
        /// </summary>
        /// <param name="html">html代码</param>
        /// <returns>提取到的url</returns>
        public static string GetAllURL(this string html)
        {
            StringBuilder sb = new StringBuilder();
            Match m = Regex.Match(html.ToLower(), "<a href=(.*?)>.*?</a>");

            while (m.Success)
            {
                sb.AppendLine(m.Result("$1"));
                m.NextMatch();
            }

            return sb.ToString();
        }
        #endregion

        #region 4.3获取所有连接文本
        /// <summary>
        /// 4.3获取所有连接文本
        /// </summary>
        /// <param name="html">html代码</param>
        /// <returns>所有的带链接的a标签</returns>
        public static string GetAllLinkText(this string html)
        {
            StringBuilder sb = new StringBuilder();
            Match m = Regex.Match(html.ToLower(), "<a href=.*?>(1,100})</a>");

            while (m.Success)
            {
                sb.AppendLine(m.Result("$1"));
                m.NextMatch();
            }

            return sb.ToString();
        }
        #endregion
        #endregion

        #region  5、匹配页面的图片地址

        /// <summary>
        /// 替换html的img路径为绝对路径
        /// </summary>
        /// <param name="html"></param>
        /// <param name="imgDest"></param>
        /// <returns></returns>
        public static string ReplaceHtmlImgSource(this string html, string imgDest) => html.Replace("<img src=\"", "<img src=\"" + imgDest + "/");

        /// <summary>
        /// 匹配页面的图片地址
        /// </summary>
        /// <param name="HtmlCode">html代码</param>
        /// <param name="imgHttp">要补充的http://路径信息</param>
        public static string GetImgSrc(this string HtmlCode, string imgHttp)
        {
            string MatchVale = "";
            string Reg = @"<img.+?>";
            foreach (Match m in Regex.Matches(HtmlCode.ToLower(), Reg))
            {
                MatchVale += GetImg((m.Value).ToLower().Trim(), imgHttp) + "|";
            }

            return MatchVale;
        }

        /// <summary>
        /// 将src的绝对路径换成相对路径
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ConvertImgSrcToRelativePath(this string s)
        {
            return Regex.Replace(s, @"<img src=""(http:\/\/.+?)/", @"<img src=""/");
        }


        /// <summary>
        /// 匹配html的img标签
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static Match MatchImgTag(this string html) => Regex.Match(html, @"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>");

        /// <summary>
        /// 获取html中第一个img标签的src
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string MatchFirstImgSrc(this string html) => MatchImgTag(html).Groups[1].Value;

        /// <summary>
        /// 随机获取html代码中的img标签的src属性
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string MatchRandomImgSrc(this string html)
        {
            GroupCollection groups = MatchImgTag(html).Groups;
            string img = groups[new Random().StrictNext(groups.Count)].Value;
            if (img.StartsWith("<"))
            {
                return img.MatchImgTag().Groups[1].Value;
            }
            return img;
        }

        /// <summary>
        /// 匹配<img src="" />中的图片路径实际链接
        /// </summary>
        /// <param name="ImgString"><img src="" />字符串</param>
        /// <param name="imgHttp">图片路径</param>
        public static string GetImg(this string ImgString, string imgHttp)
        {
            string MatchVale = "";
            string Reg = @"src=.+\.(bmp|jpg|gif|png|)";
            foreach (Match m in Regex.Matches(ImgString.ToLower(), Reg))
            {
                MatchVale += (m.Value).ToLower().Trim().Replace("src=", "");
            }
            if (MatchVale.IndexOf(".net") != -1 || MatchVale.IndexOf(".com") != -1 || MatchVale.IndexOf(".org") != -1 || MatchVale.IndexOf(".cn") != -1 || MatchVale.IndexOf(".cc") != -1 || MatchVale.IndexOf(".info") != -1 || MatchVale.IndexOf(".biz") != -1 || MatchVale.IndexOf(".tv") != -1)
                return MatchVale;
            else
                return imgHttp + MatchVale;
        }
        #endregion

        #region 6、抓取远程页面内容

        /// <summary>
        /// 6.1以GET方式抓取远程页面内容
        /// </summary>
        /// <param name="_"></param>
        /// <param name="tUrl">URL</param>
        public static string Get_Http(this HttpWebRequest _, string tUrl)
        {
            string strResult;
            try
            {
                HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(tUrl);
                hwr.Timeout = 19600;
                HttpWebResponse hwrs = (HttpWebResponse)hwr.GetResponse();
                Stream myStream = hwrs.GetResponseStream();
                StreamReader sr = new StreamReader(myStream, Encoding.Default);
                StringBuilder sb = new StringBuilder();
                while (-1 != sr.Peek())
                {
                    sb.Append(sr.ReadLine() + "\r\n");
                }
                strResult = sb.ToString();
                hwrs.Close();
            }
            catch (Exception ee)
            {
                strResult = ee.Message;
            }
            return strResult;
        }

        /// <summary>
        /// 6.2以POST方式抓取远程页面内容
        /// </summary>
        /// <param name="_"></param>
        /// <param name="url">URL</param>
        /// <param name="postData">参数列表</param>
        /// <param name="encodeType">编码类型</param>
        public static string Post_Http(this HttpWebRequest _, string url, string postData, string encodeType)
        {
            string strResult = null;
            try
            {
                Encoding encoding = Encoding.GetEncoding(encodeType);
                byte[] POST = encoding.GetBytes(postData);
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
                myRequest.Method = "POST";
                myRequest.ContentType = "application/x-www-form-urlencoded";
                myRequest.ContentLength = POST.Length;
                Stream newStream = myRequest.GetRequestStream();
                newStream.Write(POST, 0, POST.Length); //设置POST
                newStream.Close();
                HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
                StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.Default);
                strResult = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                strResult = ex.Message;
            }
            return strResult;
        }
        #endregion

        #region 7、压缩HTML输出
        /// <summary>
        /// 7.1压缩HTML输出
        /// </summary>
        /// <param name="Html">html</param>
        public static string ZipHtml(this string Html)
        {
            Html = Regex.Replace(Html, @">\s+?<", "><");//去除HTML中的空白字符
            Html = Regex.Replace(Html, @"\r\n\s*", "");
            Html = Regex.Replace(Html, @"<body([\s|\S]*?)>([\s|\S]*?)</body>", @"<body$1>$2</body>", RegexOptions.IgnoreCase);
            return Html;
        }
        #endregion

        #region 8、过滤HTML标签
        #region 8.1过滤指定HTML标签

        /// <summary>
        /// 8.1过滤指定HTML标签
        /// </summary>
        /// <param name="s_TextStr">要过滤的字符</param>
        /// <param name="html_Str">a img p div</param>
        public static string DelHtml(this string s_TextStr, string html_Str)
        {
            string rStr = "";
            if (!string.IsNullOrEmpty(s_TextStr))
            {
                rStr = Regex.Replace(s_TextStr, "<" + html_Str + "[^>]*>", "", RegexOptions.IgnoreCase);
                rStr = Regex.Replace(rStr, "</" + html_Str + ">", "", RegexOptions.IgnoreCase);
            }
            return rStr;
        }
        #endregion
        #region 8.2过滤HTML中的不安全标签

        /// <summary>
        /// 8.2过滤HTML中的不安全标签，去掉尖括号
        /// </summary>
        /// <param name="content">html代码</param>
        /// <returns>过滤后的安全内容</returns>
        public static string RemoveUnsafeHtml(this string content)
        {
            content = Regex.Replace(content, @"(\<|\s+)o([a-z]+\s?=)", "$1$2", RegexOptions.IgnoreCase);
            content = Regex.Replace(content, @"(script|frame|form|meta|behavior|style)([\s|:|>])+", "$1.$2", RegexOptions.IgnoreCase);
            return content;
        }
        #endregion
        #endregion

        #region 转换HTML操作

        #region HTML转行成TEXT
        /// <summary>
        /// HTML转行成TEXT HtmlToTxt(string strHtml)
        /// </summary>
        /// <param name="strHtml">html代码</param>
        /// <returns>普通文本</returns>
        public static string HtmlToTxt(this string strHtml)
        {
            string[] aryReg ={
            @"<script[^>]*?>.*?</script>",
            @"<(\/\s*)?!?((\w+:)?\w+)(\w+(\s*=?\s*(([""'])(\\[""'tbnr]|[^\7])*?\7|\w+)|.{0})|\s)*?(\/\s*)?>",
            @"([\r\n])[\s]+",
            @"&(quot|#34);",
            @"&(amp|#38);",
            @"&(lt|#60);",
            @"&(gt|#62);",
            @"&(nbsp|#160);",
            @"&(iexcl|#161);",
            @"&(cent|#162);",
            @"&(pound|#163);",
            @"&(copy|#169);",
            @"&#(\d+);",
            @"-->",
            @"<!--.*\n"
            };

            string strOutput = strHtml;
            for (int i = 0; i < aryReg.Length; i++)
            {
                Regex regex = new Regex(aryReg[i], RegexOptions.IgnoreCase);
                strOutput = regex.Replace(strOutput, string.Empty);
            }
            strOutput.Replace("<", "");
            strOutput.Replace(">", "");
            strOutput.Replace("\r\n", "");
            return strOutput;
        }
        #endregion

        #region 字符串转换为 Html
        /// <summary>
        /// 字符串转换为 HtmlStringToHtml(string str)
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>html标签</returns>
        public static string StringToHtml(this string str)
        {
            str = str.Replace("&", "&amp;");
            str = str.Replace(" ", "&nbsp;");
            str = str.Replace("'", "''");
            str = str.Replace("\"", "&quot;");
            str = str.Replace("<", "&lt;");
            str = str.Replace(">", "&gt;");
            str = str.Replace("\n", "<br />");
            str = str.Replace("\r", "<br />");
            str = str.Replace("\r\n", "<br />");
            return str;
        }
        #endregion

        #region Html转换成字符串
        /// <summary>
        /// html转换成字符串
        /// </summary>
        /// <param name="strHtml">html代码</param>
        /// <returns>安全的字符串</returns>
        public static string HtmlToString(this string strHtml)
        {
            strHtml = strHtml.Replace("<br>", "\r\n");
            strHtml = strHtml.Replace(@"<br />", "\r\n");
            strHtml = strHtml.Replace(@"<br/>", "\r\n");
            strHtml = strHtml.Replace("&gt;", ">");
            strHtml = strHtml.Replace("&lt;", "<");
            strHtml = strHtml.Replace("&nbsp;", " ");
            strHtml = strHtml.Replace("&quot;", "\"");
            strHtml = Regex.Replace(strHtml, @"<\/?[^>]+>", "", RegexOptions.IgnoreCase);
            return strHtml;
        }
        #endregion
        #endregion

        #region 获取URL编码

        /// <summary>
        /// 获取URL编码
        /// </summary>
        /// <param name="_"></param>
        /// <param name="url">URL</param>
        /// <returns>编码类型</returns>
        public static string GetEncoding(this HttpWebRequest _, string url)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 20000;
                request.AllowAutoRedirect = false;
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK && response.ContentLength < 1024 * 1024)
                {
                    if (response.ContentEncoding?.Equals("gzip", StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        reader = new StreamReader(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress));
                    }
                    else
                    {
                        reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII);
                    }

                    string html = reader.ReadToEnd();
                    Regex reg_charset = new Regex(@"charset\b\s*=\s*(?<charset>[^""]*)");
                    if (reg_charset.IsMatch(html))
                    {
                        return reg_charset.Match(html).Groups["charset"].Value;
                    }
                    else if (response.CharacterSet != string.Empty)
                    {
                        return response.CharacterSet;
                    }
                    else
                    {
                        return Encoding.Default.BodyName;
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
                if (reader != null)
                    reader.Close();
                if (request != null)
                    request = null;
            }
            return Encoding.Default.BodyName;
        }
        #endregion

        #region 判断URL是否有效

        /// <summary>
        /// 判断URL是否有效
        /// </summary>
        /// <param name="_"></param>
        /// <param name="url">待判断的URL，可以是网页以及图片链接等</param>
        /// <returns>200为正确，其余为大致网页错误代码</returns>
        public static int GetUrlError(this HttpWebRequest _, string url)
        {
            int num = 200;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
                ServicePointManager.Expect100Continue = false;
                ((HttpWebResponse)request.GetResponse()).Close();
            }
            catch (WebException exception)
            {
                if (exception.Status != WebExceptionStatus.ProtocolError)
                {
                    return num;
                }
                if (exception.Message.IndexOf("500 ") > 0)
                {
                    return 500;
                }
                if (exception.Message.IndexOf("401 ") > 0)
                {
                    return 401;
                }
                if (exception.Message.IndexOf("404") > 0)
                {
                    num = 404;
                }
            }
            catch
            {
                num = 401;
            }
            return num;
        }
        #endregion

        #region 返回 HTML 字符串的编码解码结果
        /// <summary>
        /// 返回 HTML 字符串的编码结果
        /// </summary>
        /// <param name="inputData">字符串</param>
        /// <returns>编码结果</returns>
        public static string HtmlEncode(string inputData)
        {
            return HttpUtility.HtmlEncode(inputData);
        }

        /// <summary>
        /// 返回 HTML 字符串的解码结果
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>解码结果</returns>
        public static string HtmlDecode(string str)
        {
            return HttpUtility.HtmlDecode(str);
        }
        #endregion

        /// <summary>
        /// 获取Cookie集合
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="cookieString">Cookie的键</param>
        /// <returns>Cookie键值集合</returns>
        public static CookieCollection GetCookieCollection(this CookieCollection cookie, string cookieString)
        {
            //string cookieString = "SID=ARRGy4M1QVBtTU-ymi8bL6X8mVkctYbSbyDgdH8inu48rh_7FFxHE6MKYwqBFAJqlplUxq7hnBK5eqoh3E54jqk=;Domain=.google.com;Path=/,LSID=AaMBTixN1MqutGovVSOejyb8mVkctYbSbyDgdH8inu48rh_7FFxHE6MKYwqBFAJqlhCe_QqxLg00W5OZejb_UeQ=;Domain=www.google.com;Path=/accounts";
            Regex re = new Regex("([^;,]+)=([^;,]+);Domain=([^;,]+);Path=([^;,]+)", RegexOptions.IgnoreCase);
            foreach (Match m in re.Matches(cookieString))
            {
                //name,   value,   path,   domain   
                Cookie c = new Cookie(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value, m.Groups[3].Value);
                cookie.Add(c);
            }
            return cookie;
        }

        #region 从HTML中获取文本,保留br,p,img

        /// <summary>
        /// 从HTML中获取文本,保留br,p,img
        /// </summary>
        /// <param name="HTML">html代码</param>
        /// <returns>保留br,p,img的文本</returns>
        public static string GetTextFromHTML(this string HTML)
        {
            Regex regEx = new Regex(@"</?(?!br|/?p|img)[^>]*>", RegexOptions.IgnoreCase);

            return regEx.Replace(HTML, "");
        }
        #endregion

        #region 获取HTML页面内制定Key的Value内容

        /// <summary>
        /// 获取HTML页面内制定Key的Value内容
        /// </summary>
        /// <param name="html">html源代码</param>
        /// <param name="key">键</param>
        /// <returns>获取到的值</returns>
        public static string GetHiddenKeyValue(this string html, string key)
        {
            string result = "";
            string sRegex = string.Format("<input\\s*type=\"hidden\".*?name=\"{0}\".*?\\s*value=[\"|'](?<value>.*?)[\"|'^/]", key);
            Regex re = new Regex(sRegex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            Match mc = re.Match(html);
            if (mc.Success)
            {
                result = mc.Groups[1].Value;
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 替换回车换行符为html换行符
        /// </summary>
        /// <param name="str">html</param>
        public static string StrFormat(this string str)
        {
            string str2;
            if (str == null)
            {
                str2 = "";
            }
            else
            {
                str = str.Replace("\r\n", "<br />");
                str = str.Replace("\n", "<br />");
                str2 = str;
            }
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

        /// <summary>
        /// 为脚本替换特殊字符串
        /// </summary>
        /// <param name="str"> </param>
        /// <returns> </returns>
        [Obsolete("不建议使用", true)]
        public static string ReplaceStrToScript(string str)
        {
            str = str.Replace("\\", "\\\\");
            str = str.Replace("'", "\\'");
            str = str.Replace("\"", "\\\"");
            return str;
        }
    }
}