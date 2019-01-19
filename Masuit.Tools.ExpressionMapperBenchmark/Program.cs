using AutoMapper;
using Masuit.Tools.Mapping;
using Masuit.Tools.Systems;
using System;
using System.Collections.Generic;

namespace Masuit.Tools.ExpressionMapperBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            #region 配置automapper

            Mapper.Initialize(e =>
            {
                e.CreateMap<TestClassA, TestClassB>().ReverseMap();
                e.CreateMap<TestClassC, TestClassD>().ReverseMap();
            });

            #endregion

            #region 配置ExpressionMapper

            ExpressionMapper.CreateMap<TestClassA, TestClassB>().ReverseMap();
            ExpressionMapper.CreateMap<TestClassC, TestClassD>().ReverseMap();

            #endregion

            #region 造一个大对象

            var a = new TestClassA()
            {
                MyProperty = "ssssssssssssssssssssss",
                DateTime = DateTime.Now,
                Double = 123.33,
                Int = 100,
                TestClassC = new TestClassC()
                {
                    MyProperty = "ccccccccccccccccccccccccccc",
                    DateTime = DateTime.Now,
                    Double = 2345.555,
                    Int = 10100,
                    Obj = new TestClassD()
                    {
                        MyProperty = "ddddddddddddddddddddddddd",
                        Obj = new TestClassC()
                        {
                            MyProperty = "cccccc",
                            DateTime = DateTime.Now,
                            Double = 23458894.555,
                            Int = 10100000,
                            Obj = new TestClassD()
                        }
                    }
                },
                List = new List<TestClassC>()
                {
                    new TestClassC()
                    {
                        MyProperty = "cccccc",
                        DateTime = DateTime.Now,
                        Double = 2345.555,
                        Int = 10100,
                        Obj = new TestClassD()
                        {
                            MyProperty = "ddddddddddddddddddddddddddddddddddd",
                            DateTime = DateTime.Now,
                            Double = 2345.555,
                            Int = 10100,
                            Obj = new TestClassC()
                            {
                                MyProperty = "cccccccccccccccccccccccccccccc",
                                DateTime = DateTime.Now,
                                Double = 2345.555,
                                Int = 10100,
                                Obj = new TestClassD()
                            }
                        }
                    },
                    new TestClassC()
                    {
                        MyProperty = "cccccc",
                        DateTime = DateTime.Now,
                        Double = 2345.555,
                        Int = 10100,
                        Obj = new TestClassD()
                        {
                            MyProperty = "ddddddddddddddddddddddddddddddddddd",
                            DateTime = DateTime.Now,
                            Double = 2345.555,
                            Int = 10100,
                            Obj = new TestClassC()
                            {
                                MyProperty = "cccccccccccccccccccccccccccccc",
                                DateTime = DateTime.Now,
                                Double = 2345.555,
                                Int = 10100,
                                Obj = new TestClassD()
                            }
                        }
                    },
                    new TestClassC()
                    {
                        MyProperty = "cccccc",
                        DateTime = DateTime.Now,
                        Double = 2345.555,
                        Int = 10100,
                        Obj = new TestClassD()
                        {
                            MyProperty = "ddddddddddddddddddddddddddddddddddd",
                            DateTime = DateTime.Now,
                            Double = 2345.555,
                            Int = 10100,
                            Obj = new TestClassC()
                            {
                                MyProperty = "cccccccccccccccccccccccccccccc",
                                DateTime = DateTime.Now,
                                Double = 2345.555,
                                Int = 10100,
                                Obj = new TestClassD()
                            }
                        }
                    },
                    new TestClassC()
                    {
                        MyProperty = "cccccc",
                        DateTime = DateTime.Now,
                        Double = 2345.555,
                        Int = 10100,
                        Obj = new TestClassD()
                        {
                            MyProperty = "ddddddddddddddddddddddddddddddddddd",
                            DateTime = DateTime.Now,
                            Double = 2345.555,
                            Int = 10100,
                            Obj = new TestClassC()
                            {
                                MyProperty = "cccccccccccccccccccccccccccccc",
                                DateTime = DateTime.Now,
                                Double = 2345.555,
                                Int = 10100,
                                Obj = new TestClassD()
                            }
                        }
                    },
                    new TestClassC()
                    {
                        MyProperty = "cccccc",
                        DateTime = DateTime.Now,
                        Double = 2345.555,
                        Int = 10100,
                        Obj = new TestClassD()
                        {
                            MyProperty = "ddddddddddddddddddddddddddddddddddd",
                            DateTime = DateTime.Now,
                            Double = 2345.555,
                            Int = 10100,
                            Obj = new TestClassC()
                            {
                                MyProperty = "cccccccccccccccccccccccccccccc",
                                DateTime = DateTime.Now,
                                Double = 2345.555,
                                Int = 10100,
                                Obj = new TestClassD()
                            }
                        }
                    },
                }
            };

            #endregion

            var time = HiPerfTimer.Execute(() =>
            {
                a.Map<TestClassA, TestClassB>();
                a.Map<TestClassA, TestClassB>();
            });
            Console.WriteLine($"ExpressionMapper映射2次耗时：{time}s");
            time = HiPerfTimer.Execute(() =>
            {
                for (int i = 0; i < 1000000; i++)
                {
                    var b = a.Map<TestClassA, TestClassB>();
                }
            });
            Console.WriteLine($"ExpressionMapper映射100000次耗时：{time}s");

            time = HiPerfTimer.Execute(() =>
            {
                Mapper.Map<TestClassB>(a);
                Mapper.Map<TestClassB>(a);
            });
            Console.WriteLine($"AutoMapper映射2次耗时：{time}s");
            time = HiPerfTimer.Execute(() =>
            {
                for (int i = 0; i < 1000000; i++)
                {
                    var b = Mapper.Map<TestClassB>(a);
                }
            });
            Console.WriteLine($"AutoMapper映射100000次耗时：{time}s");
        }
    }
}