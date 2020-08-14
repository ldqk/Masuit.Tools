using Masuit.Tools.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Masuit.Tools.Test
{
    [TestClass]
    public class TemplateTest
    {
        [TestMethod]
        public void Render_Template()
        {
            var tmp = new Template("{{name}}，你好！");
            tmp.Set("name", "万金油");
            string s = tmp.Render();
            Assert.AreEqual("万金油，你好！", s);
        }

        [TestMethod]
        public void Render_TemplateWithMultiVariables()
        {
            var tmp = new Template("{{one}},{{two}},{{three}}");
            string s = tmp.Set("one", "1").Set("two", "2").Set("three", "3").Render();
            Assert.AreEqual("1,2,3", s);
        }

        [TestMethod]
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
                Assert.IsInstanceOfType(e, typeof(ArgumentException));
                Assert.AreEqual("模版变量{{greet}}未被使用", e.Message);
            }
        }
    }
}