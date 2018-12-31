using Masuit.Tools.UnitTest.TestClasses;
using System.Collections.Generic;
using Xunit;

namespace Masuit.Tools.UnitTest
{
    public class ExpressionGenericMapperTest
    {
        /// <summary>
        /// 测试拷贝一个对象本身
        /// </summary>
        [Fact]
        public void TestSelfCopy()
        {
            TestClassA a = new TestClassA()
            {
                TestClassC = new TestClassC()
                {
                    MyProperty = "string"
                },
                List = new List<TestClassC>()
                {
                    new TestClassC(){MyProperty = "cstring"}
                }
            };

            var a2 = a.Copy();

            Assert.Equal(a.TestClassC.MyProperty, a2.TestClassC.MyProperty);
            Assert.Equal(a.List.Count, a2.List.Count);
        }

        /// <summary>
        /// 测试拷贝简单的属性
        /// </summary>
        [Fact]
        public void TestSimpleProperties()
        {
            TestClassA a = new TestClassA
            {
                MyProperty = "string"
            };

            TestClassB b = a.Map<TestClassA, TestClassB>();
            Assert.Equal(a.MyProperty, b.MyProperty);
        }

        /// <summary>
        /// 测试拷贝引用类型的属性
        /// </summary>
        [Fact]
        public void TestRefTypeProperties()
        {
            TestClassA a = new TestClassA()
            {
                TestClassC = new TestClassC()
                {
                    MyProperty = "string"
                },
                List = new List<TestClassC>()
                {
                    new TestClassC(){MyProperty = "cstring"}
                }
            };
            var b = a.Map<TestClassA, TestClassB>();
            Assert.Equal(a.MyProperty, b.MyProperty);
            Assert.Equal(a.TestClassC.MyProperty, b.TestClassC.MyProperty);
            Assert.Equal(a.List.Count, b.List.Count);
        }

        /// <summary>
        /// 测试可遍历的属性
        /// </summary>
        [Fact]
        public void TestEnumableProperties()
        {
            TestClassA a = new TestClassA()
            {
                TestClassC = new TestClassC()
                {
                    MyProperty = "string"
                },
                List = new List<TestClassC>()
                {
                    new TestClassC(){MyProperty = "cstring"},
                    new TestClassC(){MyProperty = "cstring"},
                },
                MyProperty = "string",
                Array = new[]
                {
                    new TestClassC()
                    {
                        MyProperty = "string",
                        Obj = new TestClassD()
                        {
                            MyProperty = "sstring"
                        }
                    },
                    new TestClassC()
                    {
                        MyProperty = "string",
                        Obj = new TestClassD()
                        {
                            MyProperty = "sstring"
                        }
                    },
                }
            };
            var b = a.Map<TestClassA, TestClassB>();
            Assert.Equal(a.MyProperty, b.MyProperty);
            Assert.Equal(a.TestClassC.MyProperty, b.TestClassC.MyProperty);
            Assert.Equal(a.List.Count, b.List.Count);
            Assert.Equal(a.Array.Length, b.Array.Length);
        }

        ///// <summary>
        ///// 测试HashSet的拷贝
        ///// </summary>
        //[Fact]
        //public void TestHashSet()
        //{
        //    ClassF f = new ClassF
        //    {
        //        ClassACollection = new HashSet<ClassA>
        //        {
        //            new ClassA
        //            {
        //                ValueTypeProp = 123,
        //                StringProp = "test"
        //            }
        //        }
        //    };

        //    var f2 = f.Copy();

        //    Assert.NotSame(f.ClassACollection, f2.ClassACollection);

        //    Assert.Equal(f.ClassACollection.Select(a => a.StringProp), f2.ClassACollection.Select(a => a.StringProp));

        //}


        ///// <summary>
        ///// 测试向已存在的对象拷贝属性值
        ///// </summary>
        //[Fact]
        //public void TestCopyToExistingObject()
        //{
        //    ClassA a = new ClassA
        //    {
        //        ValueTypeProp = 123,
        //        StringProp = "test"
        //    };

        //    ClassA a2 = new ClassA();
        //    a.MapTo(a2);

        //    Assert.Equal(a.ValueTypeProp, a2.ValueTypeProp);
        //    Assert.Equal(a.StringProp, a2.StringProp);

        //    ClassB b = new ClassB
        //    {
        //        ValueTypeProp = 1,
        //        StringProp = "string1",
        //        ClassATypeProp = new ClassA
        //        {
        //            ValueTypeProp = 2,
        //            StringProp = "string2",
        //        }
        //    };

        //    ClassC c = new ClassC();
        //    b.MapTo(c);

        //    Assert.Equal(b.ValueTypeProp, c.ValueTypeProp);
        //    Assert.Equal(b.StringProp, c.StringProp);
        //    Assert.Equal(b.ClassATypeProp.ValueTypeProp, c.ClassATypeProp.ValueTypeProp);
        //    Assert.Equal(b.ClassATypeProp.StringProp, c.ClassATypeProp.StringProp);
        //    Assert.NotSame(b.ClassATypeProp, c.ClassATypeProp);
        //}
    }
}