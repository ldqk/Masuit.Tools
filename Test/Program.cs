using System.Collections.Generic;
using System.Linq.Expressions;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //double time = HiPerfTimer.Execute(() =>
            //{
            //    for (int i = 0; i < 1000000; i++)
            //    {
            //        A a = new A()
            //        {
            //            C = new C()
            //            {
            //                MyProperty = "c"
            //            },
            //            List = new List<C>()
            //            {
            //                new C()
            //                {
            //                    MyProperty = "cc",
            //                    Obj = new D()
            //                    {
            //                        MyProperty = "dd"
            //                    }
            //                }
            //            }
            //        };
            //        var b = a.Map<A, B>();
            //    }
            //});
            //Console.WriteLine(time);
            NewExpression newExpression = Expression.New(typeof(int[]));

        }
    }

    public class A
    {
        public C C { get; set; }
        public List<C> List { get; set; }
    }

    public class B
    {
        public C C { get; set; }
        public List<D> List { get; set; }
    }

    public class C
    {
        public string MyProperty { get; set; }
        public D Obj { get; set; }
    }

    public class D
    {
        public string MyProperty { get; set; }
        public C Obj { get; set; }
    }
}