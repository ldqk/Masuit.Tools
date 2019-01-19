using System;
using System.Collections.Generic;

namespace Masuit.Tools.ExpressionMapperBenchmark
{
    public class TestClassA
    {
        public string MyProperty { get; set; }
        public int Int { get; set; }
        public double Double { get; set; }
        public DateTime DateTime { get; set; }

        public TestClassC TestClassC { get; set; }
        public List<TestClassC> List { get; set; }
    }

    public class TestClassB
    {
        public string MyProperty { get; set; }
        public int Int { get; set; }
        public double Double { get; set; }
        public DateTime DateTime { get; set; }


        public TestClassC TestClassC { get; set; }
        public List<TestClassD> List { get; set; }
    }

    public class TestClassC
    {
        public string MyProperty { get; set; }
        public int Int { get; set; }
        public double Double { get; set; }
        public DateTime DateTime { get; set; }

        public TestClassD Obj { get; set; }
    }

    public class TestClassD
    {
        public string MyProperty { get; set; }
        public int Int { get; set; }
        public double Double { get; set; }
        public DateTime DateTime { get; set; }

        public TestClassC Obj { get; set; }
    }
}