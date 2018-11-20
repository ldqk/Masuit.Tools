using System;
using System.Text.RegularExpressions;

namespace Masuit.Tools.Strings
{
    public class Template
    {
        private string Content { get; set; }

        public Template(string content)
        {
            Content = content;
        }

        public Template Set(string key, string value)
        {
            Content = Content.Replace("{{" + key + "}}", value);
            return this;
        }

        public string Render()
        {
            var mc = Regex.Matches(Content, @"\{\{.+?\}\}");
            foreach (Match m in mc)
            {
                throw new ArgumentException($"模版变量{m.Value}未被使用");
            }
            return Content;
        }

    }
}