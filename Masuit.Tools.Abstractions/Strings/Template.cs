using System;
using System.Text.RegularExpressions;

namespace Masuit.Tools.Strings
{
    /// <summary>
    /// 模版引擎
    /// </summary>
    public class Template
    {
        private string Content { get; set; }

        /// <summary>
        /// 模版引擎
        /// </summary>
        /// <param name="content"></param>
        public Template(string content)
        {
            Content = content;
        }

        /// <summary>
        /// 创建模板
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static Template Create(string content)
        {
            return new Template(content);
        }

        /// <summary>
        /// 设置变量
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Template Set(string key, string value)
        {
            Content = Content.Replace("{{" + key + "}}", value);
            return this;
        }

        /// <summary>
        /// 渲染模板
        /// </summary>
        /// <param name="check">是否检查未使用的模板变量</param>
        /// <returns></returns>
        public string Render(bool check = false)
        {
            if (check)
            {
                var mc = Regex.Matches(Content, @"\{\{.+?\}\}");
                foreach (Match m in mc)
                {
                    throw new ArgumentException($"模版变量{m.Value}未被使用");
                }
            }

            return Content;
        }
    }
}