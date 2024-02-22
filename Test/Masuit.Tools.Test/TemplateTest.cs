using Masuit.Tools.Strings;
using System;
using Xunit;

namespace Masuit.Tools.Test
{
    public class TemplateTest
    {
        [Fact]
        public void Render_Template()
        {
            var tmp = new Template("{{name}}，你好！");
            tmp.Set("name", "万金油");
            string s = tmp.Render();
            Assert.Equal("万金油，你好！", s);
        }

        [Fact]
        public void Render_TemplateWithMultiVariables()
        {
            var tmp = new Template("{{one}},{{two}},{{three}}");
            string s = tmp.Set("one", "1").Set("two", "2").Set("three", "3").Render();
            Assert.Equal("1,2,3", s);
        }

        [Fact]
        public void Render_TemplateWithUncoverVariable()
        {
            var tmp = new Template("{{name}}，{{greet}}！");
            tmp.Set("name", "万金油");
            try
            {
                string s = tmp.Render();
            }
            catch (Exception e)
            {
                Assert.IsType<ArgumentException>(e);
                Assert.Equal("模版变量{{greet}}未被使用", e.Message);
            }
        }
    }
}