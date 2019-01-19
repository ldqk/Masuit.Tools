using Masuit.Tools.Mapping;
using Masuit.Tools.Mapping.Core;
using Masuit.Tools.Mapping.Exceptions;
using Masuit.Tools.UnitTest.Mapping.ClassTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Masuit.Tools.UnitTest.Mapping
{
    [TestClass]
    public class MapperTests
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            Clean();
            ExpressionMapper.CreateMap<ClassSource, ClassDest>().ForMember(s => s.PropString1, d => d.PropString2);
        }

        [ClassCleanup]
        public static void Clean()
        {
            ExpressionMapper.Reset();
        }

        [TestMethod, TestCategory("CreateMap")]
        public void Mapper_CreateMap_NotExist_ContainerCount1()
        {
            Clean();
            ExpressionMapper.CreateMap<ClassSource, ClassDest>().ForMember(s => s.PropString1, d => d.PropString2);

            Assert.AreEqual(MapperConfigurationCollectionContainer.Instance.Count, 1);
        }

        [TestMethod, TestCategory("CreateMap")]
        public void Mapper_CreateMap_Already_Exist_ContainerCount1()
        {
            ExpressionMapper.CreateMap<ClassSource, ClassDest>();

            Assert.IsTrue(MapperConfigurationCollectionContainer.Instance.Exists(m => m.SourceType == typeof(ClassSource) && m.TargetType == typeof(ClassDest)));
        }

        [TestMethod, TestCategory("CreateMap")]
        public void Mapper_CreateMap_With_Name()
        {
            ExpressionMapper.CreateMap<ClassSource, ClassDest>("test");
            Assert.IsTrue(MapperConfigurationCollectionContainer.Instance.Exists(m => m.Name == "test"));
        }

        [TestMethod, TestCategory("Map")]
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

            Assert.AreEqual(actual.PropInt1, expected.PropInt1);
            Assert.AreEqual(actual.PropString2, expected.PropString1);
            Assert.AreEqual(actual.PropInt2, 0);
        }

        [TestMethod, TestCategory("GetQueryExpression")]
        public void GetQueryExpression_ReturnExpression()
        {
            Clean();
            ExpressionMapper.CreateMap<ClassSource, ClassDest>().ForMember(s => s.PropString1, d => d.PropString2);
            ExpressionMapper.Initialize();

            var actual = ExpressionMapper.GetQueryExpression<ClassSource, ClassDest>();

            Assert.IsNotNull(actual);
        }

        [TestMethod, TestCategory("GetQuery")]
        public void GetQuery_ReturnFunc()
        {
            Init(null);
            ExpressionMapper.Initialize();

            var actual = ExpressionMapper.GetQuery<ClassSource, ClassDest>();

            Assert.IsNotNull(actual);
        }

        [TestMethod, TestCategory("Exception"), ExpectedException(typeof(NoFoundMapperException))]
        public void Map_NoFoundMapperException_Exception()
        {
            new ClassSource().Map<ClassSource, ClassDest2>();
        }

        [TestMethod, TestCategory("Exception"), ExpectedException(typeof(NoActionAfterMappingException))]
        public void Map_NoActionException_Exception()
        {
            ExpressionMapper.GetMapper<ClassSource, ClassDest>().AfterMap(null);
            ExpressionMapper.Initialize();
            new ClassSource().Map<ClassSource, ClassDest>();
            Clean();
        }

        [TestMethod]
        public void GetPropertiesNotMapped_ReturnProperties_Success()
        {
            ExpressionMapper.Initialize();
            var actual = ExpressionMapper.GetPropertiesNotMapped<ClassSource, ClassDest>();
            Assert.IsTrue(actual.SourceProperties.Count > 0);
            Assert.IsTrue(actual.TargetProperties.Count > 0);
        }
    }
}