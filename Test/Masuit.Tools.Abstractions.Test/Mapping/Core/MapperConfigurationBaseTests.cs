using System;
using System.Linq.Expressions;
using Masuit.Tools.Abstractions.Test.Mapping.ClassTests;
using Masuit.Tools.Mapping;
using Masuit.Tools.Mapping.Core;
using Masuit.Tools.Mapping.Exceptions;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Mapping.Core
{
    public class MapperConfigurationBaseTests
    {
        [Fact(DisplayName = "Constructor")]
        public void NewMapperConfigurationBase_SetProperties()
        {
            MapperConfigurationBase actual = new MapperConfiguration<ClassSource, ClassDest>("sourceTest");
            Assert.Null(actual.MemberToMapForNew);
            Assert.IsType<ClassDest>(actual.TargetType);
            Assert.IsType<ClassSource>(actual.SourceType);
        }

        [Fact(DisplayName = "GetDestinationType")]
        public void GetDestinationType_WithoutServiceConstructor()
        {
            MapperConfiguration<ClassSource, ClassDest> mapper = new MapperConfiguration<ClassSource, ClassDest>("sourceTest");
            Type actual = mapper.GetDestinationType();

            Assert.IsType<ClassDest>(actual);
        }

        [Fact(DisplayName = "GetDestinationType")]
        public void GetDestinationType_WithServiceConstructor()
        {
            ExpressionMapper.ConstructServicesUsing((x) => new ClassDest2());

            MapperConfiguration<ClassSource2, IClassDest2> mapper = ExpressionMapper.CreateMap<ClassSource2, IClassDest2>().ConstructUsingServiceLocator();
            ExpressionMapper.Initialize();
            Type actual = mapper.GetDestinationType();

            Assert.IsType<ClassDest2>(actual);
            ExpressionMapper.Reset();
        }

        [Fact(DisplayName = "GetDelegate")]
        public void GetDelegate_MapperNotInitialise_Exception()
        {
            Assert.Throws<MapperNotInitializedException>(() =>
            {
                MapperConfigurationBase mapper = new MapperConfiguration<ClassSource, ClassDest>("sourceTest");
                mapper.GetDelegate();
            });
        }

        [Fact(DisplayName = "CheckAndConfigureTuple")]
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
            Assert.Null(expected.GetDelegate());
        }

        [Fact(DisplayName = "CheckAndConfigureTuple")]
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
            Assert.Null(expected.GetDelegate());
        }
    }
}