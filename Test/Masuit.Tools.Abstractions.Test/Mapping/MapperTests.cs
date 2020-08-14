using Masuit.Tools.Abstractions.Test.Mapping.ClassTests;
using Masuit.Tools.Mapping;
using Masuit.Tools.Mapping.Core;
using Masuit.Tools.Mapping.Exceptions;
using Xunit;

namespace Masuit.Tools.UnitTest.Mapping
{
    public class MapperTests
    {
        private static void Init()
        {
            Clean();
            ExpressionMapper.CreateMap<ClassSource, ClassDest>().ForMember(s => s.PropString1, d => d.PropString2);
        }

        private static void Clean()
        {
            ExpressionMapper.Reset();
        }

        [Fact(DisplayName = "CreateMap")]
        public void Mapper_CreateMap_NotExist_ContainerCount1()
        {
            Clean();
            ExpressionMapper.CreateMap<ClassSource, ClassDest>().ForMember(s => s.PropString1, d => d.PropString2);

            Assert.Equal(1, MapperConfigurationCollectionContainer.Instance.Count);
        }

        [Fact(DisplayName = "CreateMap")]
        public void Mapper_CreateMap_Already_Exist_ContainerCount1()
        {
            ExpressionMapper.CreateMap<ClassSource, ClassDest>();

            Assert.True(MapperConfigurationCollectionContainer.Instance.Exists(m => m.SourceType == typeof(ClassSource) && m.TargetType == typeof(ClassDest)));
        }

        [Fact(DisplayName = "CreateMap")]
        public void Mapper_CreateMap_With_Name()
        {
            ExpressionMapper.CreateMap<ClassSource, ClassDest>("test");
            Assert.True(MapperConfigurationCollectionContainer.Instance.Exists(m => m.Name == "test"));
        }

        [Fact(DisplayName = "CreateMap")]
        public void Map_ReturnDestinationObject_Success()
        {
            ExpressionMapper.Initialize();

            ClassSource expected = new ClassSource()
            {
                PropInt1 = 1,
                PropSourceInt1 = 1,
                PropString1 = "test"
            };

            var actual = expected.Map<ClassSource, ClassDest>();

            Assert.Equal(actual.PropInt1, expected.PropInt1);
            Assert.Equal(actual.PropString2, expected.PropString1);
            Assert.Equal(0, actual.PropInt2);
        }

        [Fact(DisplayName = "GetQueryExpression")]
        public void GetQueryExpression_ReturnExpression()
        {
            Clean();
            ExpressionMapper.CreateMap<ClassSource, ClassDest>().ForMember(s => s.PropString1, d => d.PropString2);
            ExpressionMapper.Initialize();

            System.Linq.Expressions.Expression<System.Func<ClassSource, ClassDest>> actual = ExpressionMapper.GetQueryExpression<ClassSource, ClassDest>();

            Assert.Null(actual);
        }

        [Fact(DisplayName = "GetQuery")]
        public void GetQuery_ReturnFunc()
        {
            Init();
            ExpressionMapper.Initialize();

            System.Func<ClassSource, ClassDest> actual = ExpressionMapper.GetQuery<ClassSource, ClassDest>();

            Assert.Null(actual);
        }

        [Fact(DisplayName = "Exception")]
        public void Map_NoFoundMapperException_Exception()
        {
            Assert.Throws<NoFoundMapperException>(() =>
            {
                new ClassSource().Map<ClassSource, ClassDest2>();
            });
        }

        [Fact(DisplayName = "Exception")]
        public void Map_NoActionException_Exception()
        {
            Assert.Throws<NoActionAfterMappingException>(() =>
            {
                ExpressionMapper.GetMapper<ClassSource, ClassDest>().AfterMap(null);
                ExpressionMapper.Initialize();
                new ClassSource().Map<ClassSource, ClassDest>();
                Clean();
            });
        }

        [Fact]
        public void GetPropertiesNotMapped_ReturnProperties_Success()
        {
            ExpressionMapper.Initialize();
            PropertiesNotMapped actual = ExpressionMapper.GetPropertiesNotMapped<ClassSource, ClassDest>();
            Assert.True(actual.SourceProperties.Count > 0);
            Assert.True(actual.TargetProperties.Count > 0);
        }
    }
}