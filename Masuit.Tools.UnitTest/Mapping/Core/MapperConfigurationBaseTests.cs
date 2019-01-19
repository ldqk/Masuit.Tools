using Masuit.Tools.Mapping;
using Masuit.Tools.Mapping.Core;
using Masuit.Tools.Mapping.Exceptions;
using Masuit.Tools.UnitTest.Mapping.ClassTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;

namespace Masuit.Tools.UnitTest.Mapping.Core
{
    [TestClass]
    public class MapperConfigurationBaseTests
    {

        [TestMethod, TestCategory("Constructor")]
        public void NewMapperConfigurationBase_SetProperties()
        {
            MapperConfigurationBase actual = new MapperConfiguration<ClassSource, ClassDest>("sourceTest");
            Assert.IsNotNull(actual.MemberToMapForNew);
            Assert.AreEqual(actual.TargetType, typeof(ClassDest));
            Assert.AreEqual(actual.SourceType, typeof(ClassSource));
        }

        [TestMethod, TestCategory("GetDestinationType")]
        public void GetDestinationType_WithoutServiceConstructor()
        {
            var mapper = new MapperConfiguration<ClassSource, ClassDest>("sourceTest");
            var actual = mapper.GetDestinationType();

            Assert.AreEqual(actual, typeof(ClassDest));
        }

        [TestMethod, TestCategory("GetDestinationType")]
        public void GetDestinationType_WithServiceConstructor()
        {
            ExpressionMapper.ConstructServicesUsing((x) => new ClassDest2());

            var mapper = ExpressionMapper.CreateMap<ClassSource2, IClassDest2>().ConstructUsingServiceLocator();
            ExpressionMapper.Initialize();
            var actual = mapper.GetDestinationType();

            Assert.AreEqual(actual, typeof(ClassDest2));
            ExpressionMapper.Reset();
        }


        [TestMethod, TestCategory("GetDelegate"), ExpectedException(typeof(MapperNotInitializedException))]
        public void GetDelegate_MapperNotInitialise_Exception()
        {
            MapperConfigurationBase mapper = new MapperConfiguration<ClassSource, ClassDest>("sourceTest");
            mapper.GetDelegate();
        }

        [TestMethod, TestCategory("CheckAndConfigureTuple")]
        public void CheckAndConfigureMappingTest_List_NotSameType_Success()
        {
            ExpressionMapper.Reset();
            ExpressionMapper.CreateMap<ClassSource2, ClassDest2>();

            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            MapperConfigurationCollectionContainer.Instance.Add(expected);
            ExpressionMapper.Initialize();
            Expression<Func<ClassSource, object>> source = s => s.ListProp;
            Expression<Func<ClassDest, object>> target = d => d.ListProp;
            Tuple<Expression, Expression, bool, string> tuple = Tuple.Create(source.Body, target.Body, true, string.Empty);
            expected.CheckAndConfigureMappingTest(tuple);
            Assert.IsNotNull(expected.GetDelegate());
        }

        [TestMethod, TestCategory("CheckAndConfigureTuple")]
        public void CheckAndConfigureMappingTest_List_SameType_Success()
        {
            ExpressionMapper.Reset();
            ExpressionMapper.CreateMap<ClassSource2, ClassDest2>();

            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            MapperConfigurationCollectionContainer.Instance.Add(expected);
            ExpressionMapper.Initialize();
            Expression<Func<ClassSource, object>> source = s => s.ListString;
            Expression<Func<ClassDest, object>> target = d => d.ListString;
            Tuple<Expression, Expression, bool, string> tuple = Tuple.Create(source.Body, target.Body, false, string.Empty);
            expected.CheckAndConfigureMappingTest(tuple);
            Assert.IsNotNull(expected.GetDelegate());
        }
    }
}
