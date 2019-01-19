using Masuit.Tools.Mapping;
using Masuit.Tools.Mapping.Core;
using Masuit.Tools.Mapping.Exceptions;
using Masuit.Tools.UnitTest.Mapping.ClassTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Masuit.Tools.UnitTest.Mapping.Core
{
    [TestClass]
    public class MapperConfigurationTests
    {

        [TestMethod, TestCategory("Ignore")]
        public void Ignore_Add_Succes()
        {
            var actual = new MapperConfigurationTestContainer();
            actual.Ignore((d) => d.PropInt1);
            Assert.AreEqual(actual.GetIgnoreCount(), 1);
        }
        [TestMethod, TestCategory("AfterMap")]
        public void AfterMap_Add_Succes()
        {
            var actual = new MapperConfigurationTestContainer();
            actual.AfterMap((s, d) =>
            {
                //Nothing To Do
            });
            Assert.AreEqual(actual.GetAfterMapActionCount(), 1);
        }
        [TestMethod, TestCategory("ExecuteAfterActions")]
        public void ExecuteAfterActions_Succes()
        {
            bool excecutedAction = false;
            var actual = new MapperConfigurationTestContainer();
            actual.AfterMap((s, d) =>
            {
                excecutedAction = true;
            });
            actual.ExecuteAfterActions(new ClassSource(), new ClassDest());
            Assert.IsTrue(excecutedAction);
        }


        [TestMethod, TestCategory("ReverseMap")]
        public void ReverseMap_Success()
        {
            ExpressionMapper.Reset();
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();

            var actual = expected.ReverseMap();

            Assert.IsInstanceOfType(actual, typeof(MapperConfiguration<ClassDest, ClassSource>));
        }

        [TestMethod, TestCategory("ReverseMap"), ExpectedException(typeof(MapperExistException))]
        public void ReverseMap_MapperAlreadyExist_Exception()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            MapperConfiguration<ClassDest, ClassSource> actual = null;

            expected.ReverseMap();

            actual = expected.ReverseMap();
            MapperConfigurationCollectionContainer.Instance.RemoveAt(1);
        }

        [TestMethod, TestCategory("Exception"), ExpectedException(typeof(NotSameTypePropertyException))]
        public void NotSameTypePropertyException_Exception()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            expected.ForMember(s => s.PropInt1, d => d.PropString2);
            expected.CreateMappingExpression(null);
        }

        [TestMethod, TestCategory("Exception"), ExpectedException(typeof(ReadOnlyPropertyException))]
        public void ReadOnlyPropertyExceptionException_Exception()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            expected.ForMember(s => s.PropInt1, d => d.RealOnlyPropInt1);
            expected.CreateMappingExpression(null);
        }

        [TestMethod, TestCategory("GetSortedExpression")]
        public void GetSortedExpression_PropertyFound_Succes()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            expected.CreateMappingExpression(null);
            var actual = expected.GetSortedExpression("PropInt1");
            Assert.IsNotNull(actual);
        }

        [TestMethod, TestCategory("GetSortedExpression"), ExpectedException(typeof(PropertyNoExistException))]
        public void GetSortedExpression_PropertyNotFound_Exception()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            expected.CreateMappingExpression(null);
            expected.GetSortedExpression("PropNotExist");
        }

        [TestMethod, TestCategory("GetMapper"), ExpectedException(typeof(NoFoundMapperException))]
        public void GetMapper_NoFoundMapperException()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            expected.GetMapperTest(typeof(string), typeof(string), true);
        }

        [TestMethod, TestCategory("CreateMappingExpression")]
        public void CreateMappingExpression_NotInitialise()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            expected.CreateMappingExpression(null);
            int actual = expected.MemberToMapForNew.Count;
            Assert.IsTrue(actual > 0);
        }


        [TestMethod, TestCategory("GetPropertyInfo")]
        public void GetPropertyInfo_PropertyFound_Success()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            Expression<Func<ClassSource, object>> exp = x => x.PropInt1;
            var actual = expected.GetPropertyInfoTest(exp);

            Assert.AreEqual(actual.Name, "PropInt1");
        }
        [TestMethod, TestCategory("GetPropertyInfo"), ExpectedException(typeof(NotImplementedException))]
        public void GetPropertyInfo_PropertyNotImplementedException()
        {
            PropertyInfo actual = null;
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            Expression<Func<ClassDest, object>> exp = x => x.PropInt1 > 0;
            actual = expected.GetPropertyInfoTest(exp);
        }
        [TestMethod, TestCategory("GetPropertyInfo"), ExpectedException(typeof(NotImplementedException))]
        public void GetPropertyInfo_PropertyNotImplementedExceptionDefault()
        {
            PropertyInfo actual = null;
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            Expression<Func<ClassDest, object>> exp = x => null;

            actual = expected.GetPropertyInfoTest(exp);

        }
        [TestMethod, TestCategory("CreateCommonMember")]
        public void CreateCommonMember_FindMapper_NotList_Success()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            ExpressionMapper.Reset();
            ExpressionMapper.CreateMap<ClassSource2, ClassDest2>();

            expected.CreateMappingExpression(null);
            var actual = expected.GetGenericLambdaExpression();
            ExpressionMapper.Reset();

        }
        [TestMethod, TestCategory("CreateCommonMember")]
        public void CreateCommonMember_IgnoreProperty()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            expected.Ignore(d => d.PropInt1);
            expected.CreateCommonMemberTest();
        }
        [TestMethod, TestCategory("CheckAndRemoveMemberDest")]
        public void CheckAndRemoveMemberDest_PropertyExist_Remove()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            expected.CreateMappingExpression(null);

            int countOri = expected.MemberToMapForNew.Count;

            expected.CheckAndRemoveMemberDestTest("PropInt1");
            Assert.AreNotEqual(countOri, expected.MemberToMapForNew.Count);
        }

        [TestMethod, TestCategory("CreateMemberAssignementForExisting")]
        public void CreateMemberAssignementForExisting_Succes()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            MapperConfigurationCollectionContainer.Instance.Add(expected);

            ExpressionMapper.CreateMap<ClassSource2, ClassDest2>();
            expected.CreateMappingExpression(null);

            Assert.IsNotNull(expected.GetDelegateForExistingTargetTest());
        }
    }

}
