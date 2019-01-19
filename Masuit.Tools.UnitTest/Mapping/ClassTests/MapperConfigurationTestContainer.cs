using Masuit.Tools.Mapping.Core;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Masuit.Tools.UnitTest.Mapping.ClassTests
{
    public class MapperConfigurationTestContainer : MapperConfiguration<ClassSource, ClassDest>
    {
        public MapperConfigurationTestContainer() : base("sTest")
        {
        }

        public int GetIgnoreCount()
        {
            return PropertiesToIgnore.Count;
        }

        public int GetAfterMapActionCount()
        {
            return actionsAfterMap.Count;
        }

        public MapperConfigurationBase GetMapperTest(Type tSource, Type tDest, bool throwExceptionOnNoFound)
        {
            return GetMapper(tSource, tDest, throwExceptionOnNoFound);
        }

        public void CheckAndConfigureMappingTest(Tuple<Expression, Expression, bool, string> configExpression)
        {
            CheckAndConfigureMapping(ref configExpression);
        }

        public PropertyInfo GetPropertyInfoTest(LambdaExpression expression)
        {
            return GetPropertyInfo(expression);
        }


        public void CreateCommonMemberTest()
        {
            CreateCommonMember();
        }

        public void CheckAndRemoveMemberDestTest(string propertyName)
        {
            CheckAndRemoveMemberDest(propertyName);
        }


        public void CreateMemberAssignementForExistingTargetTest()
        {
            CreateMemberAssignementForExistingTarget();
        }

        public Delegate GetDelegateForExistingTargetTest()
        {
            return GetDelegateForExistingTarget();
        }
    }
}