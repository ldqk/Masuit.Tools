using System.Collections.Generic;

namespace Masuit.Tools.UnitTest.TestClasses
{
    public class TestClassA
    {
        public string MyProperty { get; set; }

        public TestClassC TestClassC { get; set; }
        public List<TestClassC> List { get; set; }
        public TestClassC[] Array { get; set; }
    }

    public class TestClassB
    {
        public string MyProperty { get; set; }

        public TestClassC TestClassC { get; set; }
        public List<TestClassD> List { get; set; }
        public TestClassD[] Array { get; set; }
    }

    public class TestClassC
    {
        public string MyProperty { get; set; }
        public TestClassD Obj { get; set; }
    }

    public class TestClassD
    {
        public string MyProperty { get; set; }
        public TestClassC Obj { get; set; }
    }
}