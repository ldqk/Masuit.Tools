using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace Masuit.Tools.Html
{
    /*  使用方式         
    ListBuilder lb1=new ListBuilder("ul");
    lb1.SetTemplate("<a href=\"{#url}\" target=\"_blank\"><strong>{#sortName}</strong> </a>\r\n{#lb2}");

    ListBuilder lb2=new ListBuilder("ul");
    lb2.SetTemplate("<a href=\""+url+"?tid={#id}"+"\" target=\"_blank\">{#title}</a> <span class='date'>[{#date}]</span>");
    lb2.BindData(dt2);
    或者
    lb1.AddTemplateData("sortName",dr["sortName"].ToString());
    lb1.AddTemplateData("url",url+"#sid."+dr["id"].ToString()+"/page.1/");
    lb1.AddTemplateData("lb2",lb2.ToString());
    lb1.EndTemplateData();
    lSitemap.Text=lb1.ToString();
 */
    /// <summary>
    /// 无序列表生成器
    /// </summary>
    public class ListBuilder
    {
        private string type = "dl";
        private StringBuilder sb = new StringBuilder();
        private string template = string.Empty;
        private System.Collections.Specialized.NameValueCollection nvc = new System.Collections.Specialized.NameValueCollection();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_type">列表类型，可选"ul","dl"，默认"ul"</param>
        /// <param name="className">类名</param>
        public ListBuilder(string _type, string className)
        {
            if (_type != string.Empty) type = _type;
            if (className != string.Empty) className = " class='" + className + "'";
            sb.Append("<" + type + className + ">");
        }
        /// <summary>
        /// 以ul构造列表
        /// </summary>
        public ListBuilder()
        {
            sb.Append("<dl>");
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_type">列表类型，可选"ul","dl"，默认"ul"</param>
        public ListBuilder(string _type)
        {
            type = _type;
            sb.Append("<" + type + ">");
        }

        /// <summary>
        /// 设置模版
        /// </summary>
        /// <param name="_template">模版，以{#name}作为参数，name来自数据源datatable或者执行设置</param>
        public void SetTemplate(string _template)
        {
            template = _template;
        }

        /// <summary>
        /// 添加模版数据
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">值</param>
        public void AddTemplateData(string name, string value)
        {
            nvc.Add(name, value);
        }

        /// <summary>
        /// 模版数据添加完成后需调用此方法
        /// </summary>
        /// <param name="className">列表项（li或者dd）的css类名</param>
        public void EndTemplateData(string className)
        {
            string itemName = "dd";
            if (type != "dl")
            {
                itemName = "li";
            }
            if (className != string.Empty) className = " class='" + className + "'";
            sb.Append("<" + itemName + className + ">");
            sb.Append(GenerateString());
            sb.Append("</" + itemName + ">");
            nvc.Clear();
        }

        /// <summary>
        /// 模版数据添加完成后需调用此方法
        /// </summary>
        public void EndTemplateData()
        {
            EndTemplateData("");
        }

        /// <summary>
        /// 生成字符串返回
        /// </summary>
        /// <returns>生成后的内容</returns>
        protected string GenerateString()
        {
            Regex regex = new Regex(@"{#(\w+)}", RegexOptions.IgnoreCase);
            return regex.Replace(template, new MatchEvaluator(this.MatchEvaluator));
        }

        /// <summary>
        /// 正则匹配中的委托定义
        /// </summary>
        /// <param name="m">匹配对象</param>
        /// <returns>正则匹配中的委托定义</returns>
        protected string MatchEvaluator(Match m)
        {
            string name = m.Groups[1].Captures[0].Value;
            return nvc[name];
        }

        /// <summary>
        /// 设置标题
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="url">链接</param>
        /// <param name="target">目标窗口，可选"_blank","_self"等，默认"_self"</param>
        public void SetTitle(string text, string url, string target)
        {
            this.SetTitle(text, url, target, "");
        }
        /// <summary>
        /// 设置标题
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="url">链接</param>
        public void SetTitle(string text, string url)
        {
            this.SetTitle(text, url, "", "");
        }
        /// <summary>
        /// 设置纯文本标题
        /// </summary>
        /// <param name="text">文本</param>
        public void SetTitle(string text)
        {
            this.SetTitle(text, "", "", "");
        }
        /// <summary>
        /// 设置标题
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="url">链接</param>
        /// <param name="target">目标窗口，可选"_blank","_self"等，默认"_self"</param>
        /// <param name="className">CSS类名</param>
        public void SetTitle(string text, string url, string target, string className)
        {
            string itemName = "dt";
            if (type != "dl")
            {
                itemName = "li";
                className += " title ";
            }
            if (className != string.Empty) className = " class='" + className + "'";
            sb.Append("<" + itemName + className + ">");
            if (url?.Length == 0)
            {
                sb.Append(text);
            }
            else
            {
                if (target != string.Empty) target = " target='" + target + "'";
                sb.Append("<a href='" + url + "'" + target + ">");
                sb.Append(text);
                sb.Append("</a>");
            }
            sb.Append("</" + itemName + ">");
        }
        /// <summary>
        /// 添加一项
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="url">链接</param>
        /// <param name="target">目标窗口，可选"_blank","_self"等，默认"_self"</param>
        /// <param name="className">CSS类名</param>
        public void AddItem(string text, string url, string target, string className)
        {
            string itemName = "dd";
            if (type != "dl") itemName = "li";
            if (className != string.Empty) className = " class='" + className + "'";
            sb.Append("<" + itemName + className + ">");
            if (url?.Length == 0)
            {
                sb.Append(text);
            }
            else
            {
                if (target != string.Empty) target = " target='" + target + "'";
                sb.Append("<a href='" + url + "'" + target + ">");
                sb.Append(text);
                sb.Append("</a>");
            }
            sb.Append("</" + itemName + ">");
        }

        /// <summary>
        /// 返回列表的HTML
        /// </summary>
        /// <returns>列表的HTML</returns>
        public override string ToString()
        {
            sb.Append("</" + type + ">");
            return sb.ToString();
        }

        /// <summary>
        /// 根据模板绑定一个dataTable，使用datatable中的值来填充模板
        /// </summary>
        /// <param name="dataTable">内存表对象</param>
        public void BindData(System.Data.DataTable dataTable)
        {
            foreach (System.Data.DataRow dr in dataTable.Rows)
            {
                foreach (DataColumn column in dataTable.Columns)
                {
                    AddTemplateData(column.ColumnName, dr[column.ColumnName].ToString());
                }
                EndTemplateData();
            }
        }
    }
}
