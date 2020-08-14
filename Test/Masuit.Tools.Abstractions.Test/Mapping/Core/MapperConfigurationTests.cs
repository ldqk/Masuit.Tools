using System;
using System.Linq.Expressions;
using System.Reflection;
using Masuit.Tools.Abstractions.Test.Mapping.ClassTests;
using Masuit.Tools.Mapping;
using Masuit.Tools.Mapping.Core;
using Masuit.Tools.Mapping.Exceptions;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Mapping.Core
{
    public class MapperConfigurationTests
    {
        [Fact(DisplayName = "Ignore")]
        public void Ignore_Add_Succes()
        {
            var actual = new MapperConfigurationTestContainer();
            actual.Ignore((d) => d.PropInt1);
            Assert.Equal(1, actual.GetIgnoreCount());
        }

        [Fact(DisplayName = "AfterMap")]
        public void AfterMap_Add_Succes()
        {
            var actual = new MapperConfigurationTestContainer();
            actual.AfterMap((s, d) =>
            {
                //Nothing To Do
            });
            Assert.Equal(1, actual.GetAfterMapActionCount());
        }

        [Fact(DisplayName = "ExecuteAfterActions")]
        public void ExecuteAfterActions_Succes()
        {
            bool excecutedAction = false;
            var actual = new MapperConfigurationTestContainer();
            actual.AfterMap((s, d) =>
            {
                excecutedAction = true;
            });
            actual.ExecuteAfterActions(new ClassSource(), new ClassDest());
            Assert.True(excecutedAction);
        }

        [Fact(DisplayName = "ReverseMap")]
        public void ReverseMap_Success()
        {
            ExpressionMapper.Reset();
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();

            var actual = expected.ReverseMap();

            Assert.IsType<MapperConfiguration<ClassDest, ClassSource>>(actual);
        }

        [Fact(DisplayName = "ReverseMap")]
        public void ReverseMap_MapperAlreadyExist_Exception()
        {
            Assert.Throws<MapperExistException>(() =>
            {
                MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
                MapperConfiguration<ClassDest, ClassSource> actual = null;

                expected.ReverseMap();

                actual = expected.ReverseMap();
                MapperConfigurationCollectionContainer.Instance.RemoveAt(1);
            });
        }

        [Fact(DisplayName = "Exception")]
        public void NotSameTypePropertyException_Exception()
        {
            Assert.Throws<NotSameTypePropertyException>(() =>
            {
                MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
                expected.ForMember(s => s.PropInt1, d => d.PropString2);
                expected.CreateMappingExpression(null);
            });
        }

        [Fact(DisplayName = "Exception")]
        public void ReadOnlyPropertyExceptionException_Exception()
        {
            Assert.Throws<ReadOnlyPropertyException>(() =>
            {
                MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
                expected.ForMember(s => s.PropInt1, d => d.RealOnlyPropInt1);
                expected.CreateMappingExpression(null);
            });
        }

        [Fact(DisplayName = "GetSortedExpression")]
        public void GetSortedExpression_PropertyFound_Succes()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            expected.CreateMappingExpression(null);
            var actual = expected.GetSortedExpression("PropInt1");
            Assert.Null(actual);
        }

        [Fact(DisplayName = "GetSortedExpression")]
        public void GetSortedExpression_PropertyNotFound_Exception()
        {
            Assert.Throws<PropertyNoExistException>(() =>
            {
                MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
                expected.CreateMappingExpression(null);
                expected.GetSortedExpression("PropNotExist");
            });
        }

        [Fact(DisplayName = "GetMapper")]
        public void GetMapper_NoFoundMapperException()
        {
            Assert.Throws<NoFoundMapperException>(() =>
            {
                MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
                expected.GetMapperTest(typeof(string), typeof(string), true);
            });
        }

        [Fact(DisplayName = "CreateMappingExpression")]
        public void CreateMappingExpression_NotInitialise()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            expected.CreateMappingExpression(null);
            int actual = expected.MemberToMapForNew.Count;
            Assert.True(actual > 0);
        }

        [Fact(DisplayName = "GetPropertyInfo")]
        public void GetPropertyInfo_PropertyFound_Success()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            Expression<Func<ClassSource, object>> exp = x => x.PropInt1;
            var actual = expected.GetPropertyInfoTest(exp);

            Assert.Equal(actual.Name, "PropInt1");
        }

        [Fact(DisplayName = "GetPropertyInfo")]
        public void GetPropertyInfo_PropertyNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                PropertyInfo actual = null;
                MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
                Expression<Func<ClassDest, object>> exp = x => x.PropInt1 > 0;
                actual = expected.GetPropertyInfoTest(exp);
            });
        }

        [Fact(DisplayName = "GetPropertyInfo")]
        public void GetPropertyInfo_PropertyNotImplementedExceptionDefault()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                PropertyInfo actual = null;
                MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
                Expression<Func<ClassDest, object>> exp = x => null;

                actual = expected.GetPropertyInfoTest(exp);
            });
        }

        [Fact(DisplayName = "CreateCommonMember")]
        public void CreateCommonMember_FindMapper_NotList_Success()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            ExpressionMapper.Reset();
            ExpressionMapper.CreateMap<ClassSource2, ClassDest2>();

            expected.CreateMappingExpression(null);
            var actual = expected.GetGenericLambdaExpression();
            ExpressionMapper.Reset();
        }

        [Fact(DisplayName = "CreateCommonMember")]
        public void CreateCommonMember_IgnoreProperty()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            expected.Ignore(d => d.PropInt1);
            expected.CreateCommonMemberTest();
        }

        [Fact(DisplayName = "CheckAndRemoveMemberDest")]
        public void CheckAndRemoveMemberDest_PropertyExist_Remove()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            expected.CreateMappingExpression(null);

            int countOri = expected.MemberToMapForNew.Count;

            expected.CheckAndRemoveMemberDestTest("PropInt1");
            Assert.NotEqual(countOri, expected.MemberToMapForNew.Count);
        }

        [Fact(DisplayName = "CreateMemberAssignementForExisting")]
        public void CreateMemberAssignementForExisting_Succes()
        {
            MapperConfigurationTestContainer expected = new MapperConfigurationTestContainer();
            MapperConfigurationCollectionContainer.Instance.Add(expected);

            ExpressionMapper.CreateMap<ClassSource2, ClassDest2>();
            expected.CreateMappingExpression(null);

            Assert.Null(expected.GetDelegateForExistingTargetTest());
        }
    }
}