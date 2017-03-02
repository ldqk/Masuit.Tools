using System;
using System.Text;

namespace Masuit.Tools.Html
{
    /// <summary>
    /// HTMl 分页
    /// </summary>
    public static class HtmlPager
    {
        /// <summary>
        /// 写出分页
        /// </summary>
        /// <param name="pageCount">页数</param>
        /// <param name="currentPage">当前页</param>
        public static string GetPager(int pageCount, int currentPage)
        {
            return GetPager(pageCount, currentPage, new string[] { }, new string[] { });
        }

        /// <summary>
        /// 写出分页
        /// </summary>
        /// <param name="pageCount">页数</param>
        /// <param name="currentPage">当前页</param>
        /// <param name="FieldName">地址栏参数</param>
        /// <param name="FieldValue">地址栏参数值</param>
        /// <returns></returns>
        public static string GetPager(int pageCount, int currentPage, string[] FieldName, string[] FieldValue)
        {
            string pString = "";
            for (int i = 0; i < FieldName.Length; i++)
            {
                pString += "&" + FieldName[i] + "=" + FieldValue[i];
            }
            int stepNum = 4;
            int pageRoot = 1;
            pageCount = pageCount == 0 ? 1 : pageCount;
            currentPage = currentPage == 0 ? 1 : currentPage;

            StringBuilder sb = new StringBuilder();
            sb.Append("<table cellpadding=0 cellspacing=1 class=\"pager\">\r<tr>\r");
            sb.Append("<td class=pagerTitle>&nbsp;分页&nbsp;</td>\r");
            sb.Append("<td class=pagerTitle>&nbsp;" + currentPage + "/" + pageCount + "&nbsp;</td>\r");
            if (currentPage - stepNum < 2)
                pageRoot = 1;
            else
                pageRoot = currentPage - stepNum;
            int pageFoot = pageCount;
            if (currentPage + stepNum >= pageCount)
                pageFoot = pageCount;
            else
                pageFoot = currentPage + stepNum;
            if (pageRoot == 1)
            {
                if (currentPage > 1)
                {
                    sb.Append("<td>&nbsp;<a href='?page=1" + pString + "' title='首页'>首页</a>&nbsp;</td>\r");
                    sb.Append("<td>&nbsp;<a href='?page=" + Convert.ToString(currentPage - 1) + pString + "' title='上页'>上页</a>&nbsp;</td>\r");
                }
            }
            else
            {
                sb.Append("<td>&nbsp;<a href='?page=1" + pString + "' title='首页'>首页</a>&nbsp;</td>");
                sb.Append("<td>&nbsp;<a href='?page=" + Convert.ToString(currentPage - 1) + pString + "' title='上页'>上页</a>&nbsp;</td>\r");
            }
            for (int i = pageRoot; i <= pageFoot; i++)
            {
                if (i == currentPage)
                {
                    sb.Append("<td class='current'>&nbsp;" + i.ToString() + "&nbsp;</td>\r");
                }
                else
                {
                    sb.Append("<td>&nbsp;<a href='?page=" + i.ToString() + pString + "' title='第" + i.ToString() + "页'>" + i.ToString() + "</a>&nbsp;</td>\r");
                }
                if (i == pageCount)
                    break;
            }
            if (pageFoot == pageCount)
            {
                if (pageCount > currentPage)
                {
                    sb.Append("<td>&nbsp;<a href='?page=" + Convert.ToString(currentPage + 1) + pString + "' title='下页'>下页</a>&nbsp;</td>\r");
                    sb.Append("<td>&nbsp;<a href='?page=" + pageCount.ToString() + pString + "' title='尾页'>尾页</a>&nbsp;</td>\r");
                }
            }
            else
            {
                sb.Append("<td>&nbsp;<a href='?page=" + Convert.ToString(currentPage + 1) + pString + "' title='下页'>下页</a>&nbsp;</td>\r");
                sb.Append("<td>&nbsp;<a href='?page=" + pageCount.ToString() + pString + "' title='尾页'>尾页</a>&nbsp;</td>\r");
            }
            sb.Append("</tr>\r</table>");
            return sb.ToString();
        }

        /// <summary>
        /// 写出分页
        /// </summary>
        /// <param name="pageCount">总页数</param>
        /// <param name="currentPage">当前页</param>
        /// <param name="prefix">上一页</param>
        /// <param name="suffix">下一页</param>
        /// <returns></returns>
        public static string GetHtmlPager(int pageCount, int currentPage, string prefix, string suffix)
        {
            int stepNum = 4;
            int pageRoot = 1;
            pageCount = pageCount == 0 ? 1 : pageCount;
            currentPage = currentPage == 0 ? 1 : currentPage;
            StringBuilder sb = new StringBuilder();
            sb.Append("<table cellpadding=0 cellspacing=1 class=\"pager\">\r<tr>\r");
            sb.Append("<td class=pagerTitle>&nbsp;分页&nbsp;</td>\r");
            sb.Append("<td class=pagerTitle>&nbsp;" + currentPage.ToString() + "/" + pageCount.ToString() + "&nbsp;</td>\r");
            if (currentPage - stepNum < 2)
                pageRoot = 1;
            else
                pageRoot = currentPage - stepNum;
            int pageFoot = pageCount;
            if (currentPage + stepNum >= pageCount)
                pageFoot = pageCount;
            else
                pageFoot = currentPage + stepNum;
            if (pageRoot == 1)
            {
                if (currentPage > 1)
                {
                    sb.Append("<td>&nbsp;<a href='" + prefix + "1" + suffix + "' title='首页'>首页</a>&nbsp;</td>\r");
                    sb.Append("<td>&nbsp;<a href='" + prefix + Convert.ToString(currentPage - 1) + suffix + "' title='上页'>上页</a>&nbsp;</td>\r");
                }
            }
            else
            {
                sb.Append("<td>&nbsp;<a href='" + prefix + "1" + suffix + "' title='首页'>首页</a>&nbsp;</td>");
                sb.Append("<td>&nbsp;<a href='" + prefix + Convert.ToString(currentPage - 1) + suffix + "' title='上页'>上页</a>&nbsp;</td>\r");
            }
            for (int i = pageRoot; i <= pageFoot; i++)
            {
                if (i == currentPage)
                {
                    sb.Append("<td class='current'>&nbsp;" + i.ToString() + "&nbsp;</td>\r");
                }
                else
                {
                    sb.Append("<td>&nbsp;<a href='" + prefix + i.ToString() + suffix + "' title='第" + i.ToString() + "页'>" + i.ToString() + "</a>&nbsp;</td>\r");
                }
                if (i == pageCount)
                    break;
            }
            if (pageFoot == pageCount)
            {
                if (pageCount > currentPage)
                {
                    sb.Append("<td>&nbsp;<a href='" + prefix + Convert.ToString(currentPage + 1) + suffix + "' title='下页'>下页</a>&nbsp;</td>\r");
                    sb.Append("<td>&nbsp;<a href='" + prefix + pageCount.ToString() + suffix + "' title='尾页'>尾页</a>&nbsp;</td>\r");
                }
            }
            else
            {
                sb.Append("<td>&nbsp;<a href='" + prefix + Convert.ToString(currentPage + 1) + suffix + "' title='下页'>下页</a>&nbsp;</td>\r");
                sb.Append("<td>&nbsp;<a href='" + prefix + pageCount.ToString() + suffix + "' title='尾页'>尾页</a>&nbsp;</td>\r");
            }
            sb.Append("</tr>\r</table>");
            return sb.ToString();
        }

        #region 分页

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="para">参数</param>
        /// <param name="sumpage">总页数</param>
        /// <param name="page">当前页</param>
        /// <returns>当前页的内容</returns>
        public static string Paging(string url, string para, int sumpage, int page)
        {
            string result = string.Empty;
            if (sumpage == 1)
            {
                return result;
            }
            if (sumpage > 500)
            {
                sumpage = 500;
            }
            if (page > sumpage)
            {
                page = 1;
            }
            StringBuilder sb = new StringBuilder();
            if (sumpage > 0)
            {
                switch (page)
                {
                    case 1:
                        sb.Append(string.Format("<p class=\"next\"><a href=\"{0}?page={1}{2}\">{3}</a> ", new object[] { url, page + 1, para, "下一页" }));
                        break;
                    default:
                        if (sumpage == page)
                        {
                            sb.Append(string.Format("<p class=\"next\"><a href=\"{0}?page={1}{2}\">{3}</a> ", new object[] { url, page - 1, para, "上一页" }));
                        }
                        else
                        {
                            sb.Append(string.Format("<p class=\"next\"><a href=\"{0}?page={1}{2}\">{3}</a> <a href=\"{4}?page={5}{6}\">{7}</a> ",
                                new object[] { url, page + 1, para, "下一页", url, page - 1, para, "上一页" }));
                        }
                        break;
                }
                sb.Append(string.Format("第{0}/{1}页</p>", new object[] { page, sumpage }));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 分页控件
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="para">参数</param>
        /// <param name="sumpage">总页数</param>
        /// <param name="page">当前页</param>
        /// <param name="myPaging">分页控件</param>
        /// <returns>当前页的内容</returns>
        public static string Paging(string url, string para, int sumpage, int page, System.Web.UI.UserControl myPaging)
        {
            myPaging.Visible = false;
            string result = string.Empty;
            if (sumpage == 1)
            {
                return result;
            }
            if (sumpage > 500)
            {
                sumpage = 500;
            }
            if (page > sumpage)
            {
                page = 1;
            }
            StringBuilder sb = new StringBuilder();
            if (sumpage > 0)
            {
                myPaging.Visible = true;
                switch (page)
                {
                    case 1:
                        sb.Append(string.Format("<a href=\"{0}?page={1}{2}\">{3}</a> ", new object[] { url, page + 1, para, "下一页" }));
                        break;
                    default:
                        if (sumpage == page)
                        {
                            sb.Append(string.Format("<a href=\"{0}?page={1}{2}\">{3}</a> ", new object[] { url, page - 1, para, "上一页" }));
                        }
                        else
                        {
                            sb.Append(string.Format("<a href=\"{0}?page={1}{2}\">{3}</a> <a href=\"{4}?page={5}{6}\">{7}</a> ",
                                new object[] { url, page + 1, para, "下一页", url, page - 1, para, "上一页" }));
                        }
                        break;
                }
                sb.Append(string.Format("第{0}/{1}页", new object[] { page, sumpage }));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="para">参数</param>
        /// <param name="sumpage">总页数</param>
        /// <param name="page">当前页</param>
        /// <param name="count">总条数</param>
        /// <returns>当前页的内容</returns>
        public static string Paging(string para, int sumpage, int page, int count)
        {
            string result = string.Empty;
            if (page > sumpage)
            {
                page = 1;
            }
            StringBuilder sb = new StringBuilder();
            if (sumpage > 0)
            {
                if (sumpage != 1)
                {
                    switch (page)
                    {
                        case 1:
                            sb.Append(string.Format("<a href=\"?page={0}{1}\">{2}</a> ", new object[] { page + 1, para, "下一页" }));
                            break;
                        default:
                            if (sumpage == page)
                            {
                                sb.Append(string.Format("<a href=\"?page={0}{1}\">{2}</a> ", new object[] { page - 1, para, "上一页" }));
                            }
                            else
                            {
                                sb.Append(string.Format("<a href=\"?page={0}{1}\">{2}</a> <a href=\"?page={3}{4}\">{5}</a> ",
                                    new object[] { page - 1, para, "上一页", page + 1, para, "下一页" }));
                            }
                            break;
                    }
                }
                sb.Append(string.Format("第{0}/{1}页 共{2}条", new object[] { page, sumpage, count }));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 带第一页和最后一页
        /// </summary>
        /// <param name="para">参数</param>
        /// <param name="sumpage">总页数</param>
        /// <param name="page">当前页</param>
        /// <param name="count">总条数</param>
        /// <returns>当前页的内容</returns>
        public static string paging2(string para, int sumpage, int page, int count)
        {
            string result = string.Empty;
            if (page > sumpage)
            {
                page = 1;
            }
            StringBuilder sb = new StringBuilder();
            if (sumpage > 0)
            {
                if (sumpage != 1)
                {
                    //第一页
                    sb.Append(string.Format("<a href=\"?page={0}{1}\"><img src=\"images/first-icon.gif\" border=\"0\"/></a>&nbsp;&nbsp;", new object[] { 1, para }));
                    switch (page)
                    {
                        case 1:
                            //前一页图片
                            sb.Append(string.Format("<a>{0}</a>", new object[] { "<img src=\"images/left-icon.gif\" border=\"0\"/>" }));
                            sb.Append(string.Format("<a>上一页</a><a href=\"?page={0}{1}\">{2}</a> ", new object[] { page + 1, para, "下一页" }));
                            //后一页图片
                            sb.Append(string.Format("<a href=\"?page={0}{1}\">{2}</a>", new object[] { page + 1, para, "<img src=\"images/right-icon.gif\" border=\"0\"/>" }));
                            break;
                        default:
                            if (sumpage == page)
                            {
                                //前一页图片
                                sb.Append(string.Format("<a href=\"?page={0}{1}\">{2}</a>", new object[] { page - 1, para, "<img src=\"images/left-icon.gif\" border=\"0\"/>" }));
                                sb.Append(string.Format("<a href=\"?page={0}{1}\">{2}</a><a>下一页</a> ", new object[] { page - 1, para, "上一页" }));
                                //后一页图片
                                sb.Append(string.Format("<a>{0}</a>", new object[] { "<img src=\"images/right-icon.gif\" />" }));
                            }
                            else
                            {
                                //前一页图片
                                sb.Append(string.Format("<a href=\"?page={0}{1}\">{2}</a>", new object[] { page - 1, para, "<img src=\"images/left-icon.gif\" border=\"0\"/>" }));
                                sb.Append(string.Format("<a href=\"?page={0}{1}\">{2}</a> <a href=\"?page={3}{4}\">{5}</a> ",
                                    new object[] { page - 1, para, "上一页", page + 1, para, "下一页" }));
                                //后一页图片
                                sb.Append(string.Format("<a href=\"?page={0}{1}\">{2}</a>", new object[] { page + 1, para, "<img src=\"images/right-icon.gif\" border=\"0\"/>" }));
                            }
                            break;
                    }
                    //最后一页图片
                    sb.Append(string.Format("&nbsp;&nbsp;<a href=\"?page={0}{1}\"><img src=\"images/last-icon.gif\" border=\"0\"/></a>&nbsp;&nbsp;", new object[] { sumpage, para }));
                }
                sb.Append(string.Format("第{0}页/共{1}页 共{2}条", new object[] { page, sumpage, count }));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="para">参数</param>
        /// <param name="sumpage">总页数</param>
        /// <param name="page">当前页</param>
        /// <param name="count">总条数</param>
        /// <returns>当前页的内容</returns>
        public static string Paging(string url, string para, int sumpage, int page, int count)
        {
            string result = string.Empty;
            if (page > sumpage)
            {
                page = 1;
            }
            StringBuilder sb = new StringBuilder();
            if (sumpage > 0)
            {
                if (sumpage != 1)
                {
                    //第一页
                    sb.Append(string.Format("<a href=\"{2}?page={0}{1}\">首页</a>", new object[] { 1, para, url }));
                    switch (page)
                    {
                        case 1:
                            //前一页图片
                            // sb.Append(string.Format("<a>{0}</a>", new object[] { "<img src=\"images/left-icon.gif\" border=\"0\"/>" }));
                            sb.Append(string.Format("<a>上一页</a><a href=\"{3}?page={0}{1}\">{2}</a> ", new object[] { page + 1, para, "下一页", url }));
                            //后一页图片
                            // sb.Append(string.Format("<a href=\"?page={0}{1}\">{2}</a>", new object[] { page + 1, para, "<img src=\"images/right-icon.gif\" border=\"0\"/>" }));
                            break;
                        default:
                            if (sumpage == page)
                            {
                                //前一页图片
                                //sb.Append(string.Format("<a href=\"?page={0}{1}\">{2}</a>", new object[] { page - 1, para, "<img src=\"images/left-icon.gif\" border=\"0\"/>" }));
                                sb.Append(string.Format("<a href=\"{3}?page={0}{1}\">{2}</a><a>下一页</a> ", new object[] { page - 1, para, "上一页", url }));
                                //后一页图片
                                //sb.Append(string.Format("<a>{0}</a>", new object[] { "<img src=\"images/right-icon.gif\" />" }));
                            }
                            else
                            {
                                //前一页图片
                                //sb.Append(string.Format("<a href=\"?page={0}{1}\">{2}</a>", new object[] { page - 1, para, "<img src=\"images/left-icon.gif\" border=\"0\"/>" }));
                                sb.Append(string.Format("<a href=\"{6}?page={0}{1}\">{2}</a> <a href=\"{6}?page={3}{4}\">{5}</a> ",
                                    new object[] { page - 1, para, "上一页", page + 1, para, "下一页", url }));
                                //后一页图片
                                //sb.Append(string.Format("<a href=\"?page={0}{1}\">{2}</a>", new object[] { page + 1, para, "<img src=\"images/right-icon.gif\" border=\"0\"/>" }));
                            }
                            break;
                    }
                    //最后一页图片
                    sb.Append(string.Format("<a href=\"{2}?page={0}{1}\">末页</a>&nbsp;&nbsp;", new object[] { sumpage, para, url }));
                }
                sb.Append(string.Format("第{0}页/共{1}页 共{2}条", new object[] { page, sumpage, count }));
            }
            return sb.ToString();
        }
        #endregion

    }
}
