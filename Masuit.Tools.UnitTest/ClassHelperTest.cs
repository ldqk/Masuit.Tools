using Masuit.Tools.Reflection;
using System.Collections.Generic;
using Xunit;

namespace Masuit.Tools.UnitTest
{
    public class ClassHelperTest
    {
        [Fact]
        public void Can_AddProperty_ReturnNewInstance()
        {
            // arrange
            MyClass myClass = new MyClass();

            // act
            dynamic newObj = myClass.AddProperty(new List<ClassHelper.CustPropertyInfo>()
            {
                new ClassHelper.CustPropertyInfo(typeof(string),"Name","张三"),
                new ClassHelper.CustPropertyInfo(typeof(int),"Age",20),
            }).AddProperty("List", new List<string>());

            // act
            Assert.Equal("张三", newObj.Name);
            Assert.Equal(20, newObj.Age);
            Assert.IsType<List<string>>(newObj.List);
        }

        [Fact]
        public void Can_RemoveProperty_ReturnNewInstance()
        {
            // arrange
            MyClass myClass = new MyClass()
            {
                MyProperty = "aa",
                Number = 123
            };

            // act
            object newObj = myClass.DeleteProperty("MyProperty");

            // act
            int propertyCount = newObj.GetType().GetProperties().Length;
            Assert.Equal(1, propertyCount);
        }
    }

    public class MyClass
    {
        public string MyProperty { get; set; }
        public int Number { get; set; }

    }
}