using Masuit.Tools.Mapping.Exceptions;
using Masuit.Tools.Mapping.Helper;
using Masuit.Tools.Mapping.Visitor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Masuit.Tools.Mapping.Core
{
    /// <summary>
    /// mapper配置基类
    /// </summary>
    public abstract class MapperConfigurationBase
    {
        private Delegate _delegateCallForNew;

        private Delegate _delegateCallForExisting;

        private Func<Type, object> _constructorFunc;

        private bool _isInitialized;
        private readonly MethodInfo _selectMethod;

        private readonly MethodInfo _toListMethod;

        private readonly List<PropertyInfo> _propertiesToIgnore;

        internal ParameterExpression paramClassSource;
        internal MapperExpressionVisitor visitorMapper;
        internal List<MemberAssignment> memberForNew;
        internal LambdaExpression expressionForExisting;

        /// <summary>
        /// 属性映射对应关系<br/>
        /// Item1 : 源表达式<br/>
        /// Item2 : 目标表达式<br/>
        /// Item3 : 检查null值<br/>
        /// Item4 : mapper别名<br/>
        /// </summary>
        protected List<Tuple<Expression, Expression, bool, string>> PropertiesMapping { get; private set; }


        /// <summary>
        /// 需要被忽略映射的属性
        /// </summary>
        protected ReadOnlyCollection<PropertyInfo> PropertiesToIgnore => _propertiesToIgnore.AsReadOnly();

        /// <summary>
        /// 是否使用服务依赖注入
        /// </summary>
        public bool UseServiceLocator { get; protected set; }

        /// <summary>
        /// 对象源类型
        /// </summary>
        public Type SourceType { get; private set; }

        /// <summary>
        /// 对象目标类型
        /// </summary>
        public Type TargetType { get; private set; }

        /// <summary>
        /// 获取mapper映射成员
        /// </summary>
        public ReadOnlyCollection<MemberAssignment> MemberToMapForNew => new ReadOnlyCollection<MemberAssignment>(memberForNew);


        /// <summary>
        /// mapper别名
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="source">源类型</param>
        /// <param name="destination">目标类型</param>
        /// <param name="paramName">属性名</param>
        /// <param name="name">别名</param>
        protected MapperConfigurationBase(Type source, Type destination, string paramName, string name = null)
        {
            TargetType = destination;
            SourceType = source;
            paramClassSource = Expression.Parameter(source, paramName);
            Name = string.IsNullOrEmpty(name) ? paramName : name;
            _propertiesToIgnore = new List<PropertyInfo>();
            PropertiesMapping = new List<Tuple<Expression, Expression, bool, string>>();
            visitorMapper = new MapperExpressionVisitor(paramClassSource);
            memberForNew = new List<MemberAssignment>();
            _selectMethod = typeof(Enumerable).GetMethods().Where(m => m.Name == "Select").Select(x => x.GetParameters().First(p => p.Name.Equals("selector") && p.ParameterType.GetGenericArguments().Length == 2)).First().Member as MethodInfo;
            _toListMethod = typeof(Enumerable).GetMethod("ToList");
        }

        /// <summary>
        /// 获取mapper委托
        /// </summary>
        /// <returns></returns>
        public Delegate GetDelegate()
        {
            if (!_isInitialized)
            {
                throw new MapperNotInitializedException(SourceType, TargetType);
            }

            // 因为在这里有映射器的性能问题，而缓存委托会显着缩短处理时间，如果没有表达式编译每次编译会很慢
            if (_delegateCallForNew == null)
            {
                MemberInitExpression exp = GetMemberInitExpression();

                _delegateCallForNew = Expression.Lambda(exp, paramClassSource).Compile();
            }

            return _delegateCallForNew;
        }

        /// <summary>
        /// 获取现有目标类型的委托。
        /// </summary>
        /// <exception cref="MapperNotInitializedException"></exception>
        public Delegate GetDelegateForExistingTarget()
        {
            if (!_isInitialized)
            {
                throw new MapperNotInitializedException(SourceType, TargetType);
            }

            // 因为在这里有映射器的性能问题，而缓存委托会显着缩短处理时间，如果没有表达式编译每次编译会很慢
            if (_delegateCallForExisting == null)
            {
                CreateMemberAssignementForExistingTarget();
            }

            return _delegateCallForExisting;
        }

        /// <summary>
        /// 获取泛型的Lambda表达式
        /// </summary>
        public LambdaExpression GetGenericLambdaExpression()
        {
            MemberInitExpression exp = GetMemberInitExpression();
            return Expression.Lambda(exp, paramClassSource);
        }

        /// <summary>
        /// 获取目标类型的实际类型
        /// </summary>
        public Type GetDestinationType()
        {
            return GetRealType(TargetType);
        }

        /// <summary>
        /// 忽略目标类型的属性
        /// </summary>
        /// <typeparam name="TDest">对象源类型</typeparam>
        /// <typeparam name="TProperty">对象目标类型</typeparam>
        /// <param name="propertyDest">目标对象的属性</param>
        /// <returns></returns>
        protected MapperConfigurationBase IgnoreBase<TDest, TProperty>(Expression<Func<TDest, TProperty>> propertyDest)
        {
            // 添加到映射列表并且可以继续操作
            _propertiesToIgnore.Add(GetPropertyInfo(propertyDest));
            return this;
        }

        /// <summary>
        /// 获取映射器实例
        /// </summary>
        /// <param name="typeOfSource">源类型</param>
        /// <param name="typeOfTarget">目标类型</param>
        /// <param name="throwExceptionOnNoFound">如果没找到是否需要抛出异常</param>
        /// <param name="name">mapper别名</param>
        /// <returns></returns>
        /// <exception cref="NoFoundMapperException"></exception>
        protected static MapperConfigurationBase GetMapper(Type typeOfSource, Type typeOfTarget, bool throwExceptionOnNoFound, string name = null)
        {
            var mapperExterne = MapperConfigurationCollectionContainer.Instance.Find(typeOfSource, typeOfTarget, name);
            // 如果没有任何配置，手动抛出异常
            if (mapperExterne == null && throwExceptionOnNoFound)
            {
                throw new NoFoundMapperException(typeOfSource, typeOfTarget);
            }

            return mapperExterne;
        }

        /// <summary>
        /// 创建公共成员
        /// </summary>
        protected void CreateCommonMember()
        {
            PropertyInfo[] propertiesSource = SourceType.GetProperties();
            foreach (PropertyInfo propSource in propertiesSource)
            {
                PropertyInfo propDest = TargetType.GetProperty(propSource.Name);
                if (propDest != null)
                {
                    // 检查是否已存在或被忽略。
                    bool ignorePropDest = _propertiesToIgnore.Exists(x => x.Name == propDest.Name) || PropertiesMapping.Exists(x => GetPropertyInfo(x.Item2).Name == propDest.Name);

                    if (propDest.CanWrite && !ignorePropDest)
                    {
                        Type sourceType = propSource.PropertyType;
                        Type destType = propDest.PropertyType;
                        bool isList = IsListOf(destType);
                        if (isList)
                        {
                            sourceType = TypeSystem.GetElementType(propSource.PropertyType);
                            destType = TypeSystem.GetElementType(propDest.PropertyType);
                        }

                        var canCreateConfig = CanCreateConfig(sourceType, destType);
                        if (canCreateConfig.CanCreate)
                        {
                            // 只创造现有的关系
                            Expression expSource = Expression.MakeMemberAccess(paramClassSource, propSource);
                            ParameterExpression paramDest = Expression.Parameter(TargetType, "t");
                            Expression expDest = Expression.MakeMemberAccess(paramDest, propDest);
                            PropertiesMapping.Add(Tuple.Create(expSource, expDest, false, canCreateConfig.MapperName));
                        }
                    }
                }
            }
        }

        private static CreateConfig CanCreateConfig(Type typeSource, Type typeTarget)
        {
            CreateConfig result = new CreateConfig
            {
                CanCreate = typeSource == typeTarget
            };
            //不是同一类型
            if (!result.CanCreate)
            {
                //查找是否存在映射器
                var mapper = MapperConfigurationCollectionContainer.Instance.Find(typeSource, typeTarget);
                if (mapper != null)
                {
                    result.MapperName = mapper.Name;
                    result.CanCreate = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 检查并配置mapper
        /// </summary>
        /// <param name="configExpression">配置表达式树</param>
        /// <exception cref="NotSameTypePropertyException">
        /// </exception>
        /// <exception cref="ReadOnlyPropertyException"></exception>
        protected void CheckAndConfigureMapping(ref Tuple<Expression, Expression, bool, string> configExpression)
        {
            Type typeSource = configExpression.Item1.Type;
            Type typeTarget = configExpression.Item2.Type;

            // 正常情况下，目标表达式是一个成员表达式树
            PropertyInfo propTarget = GetPropertyInfo(configExpression.Item2);

            if (propTarget.CanWrite)
            {
                CheckAndRemoveMemberDest(propTarget.Name);
                if (!IsListOf(typeTarget))
                {
                    CreatBindingFromSimple(ref configExpression, typeSource, typeTarget, propTarget);
                }
                else
                {
                    CreateBindingFromList(ref configExpression, typeSource, typeTarget, propTarget);
                }
            }
            else
            {
                throw new ReadOnlyPropertyException(propTarget);
            }
        }

        /// <summary>
        /// 检查并移除目标成员
        /// </summary>
        /// <param name="properyName">属性名</param>
        protected void CheckAndRemoveMemberDest(string properyName)
        {
            Predicate<MemberAssignment> exp = m => m.Member.Name == properyName;
            if (memberForNew.Exists(exp))
            {
                memberForNew.RemoveAll(exp);
            }
        }

        /// <summary>
        /// 获取成员初始化表达式。
        /// </summary>
        /// <returns></returns>
        protected MemberInitExpression GetMemberInitExpression()
        {
            Type typeDest = GetDestinationType();
            NewExpression newClassDest = Expression.New(typeDest);
            MemberInitExpression exp = Expression.MemberInit(newClassDest, MemberToMapForNew);
            return exp;
        }

        /// <summary>
        /// 创建成员绑定。
        /// </summary>
        /// <param name="propertyExpression">属性表达式</param>
        /// <param name="propertyTarget">目标属性</param>
        /// <param name="checkIfNull">是否检查null值</param>
        protected void CreateMemberBinding(Expression propertyExpression, MemberInfo propertyTarget, bool checkIfNull)
        {
            // 访问表达式进行转换
            Expression result = visitorMapper.Visit(propertyExpression, checkIfNull);
            MemberAssignment bind = Expression.Bind(propertyTarget, result);
            memberForNew.Add(bind);
        }

        /// <summary>
        /// 将表达式源的映射分配给属性目标。
        /// </summary>
        /// <param name="getPropertySource">属性源类型</param>
        /// <param name="getPropertyDest">属性目标类型</param>
        /// <param name="checkIfNull">是否检查null值</param>
        /// <param name="name">要使用的映射器的别名</param>
        internal MapperConfigurationBase ForMemberBase(Expression getPropertySource, Expression getPropertyDest, bool checkIfNull, string name = null)
        {
            // 添加到映射列表并且可以继续操作
            PropertiesMapping.Add(Tuple.Create(getPropertySource, getPropertyDest, checkIfNull, name));
            return this;
        }

        /// <summary>
        /// 获取属性信息。
        /// </summary>
        /// <param name="propertyExpression">属性表达式树</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException">
        /// 这种表达方式不承担职责，或者这种类型的表达式是无效的
        /// </exception>
        protected static PropertyInfo GetPropertyInfo(Expression propertyExpression)
        {
            var expressionToAnalyse = propertyExpression.NodeType == ExpressionType.Lambda ? (propertyExpression as LambdaExpression).Body : propertyExpression;
            switch (expressionToAnalyse.NodeType)
            {
                case ExpressionType.Convert:
                    Expression operand = (expressionToAnalyse as UnaryExpression).Operand;
                    switch (operand.NodeType)
                    {
                        case ExpressionType.MemberAccess:
                            return (operand as MemberExpression).Member as PropertyInfo;
                        default:
                            throw new NotImplementedException("这种表达方式目前尚未支持");
                    }
                case ExpressionType.MemberAccess:
                    return (expressionToAnalyse as MemberExpression).Member as PropertyInfo;
                default:
                    throw new NotImplementedException("这种表达方式目前尚未支持");
            }
        }

        internal void Initialize(Func<Type, object> constructor)
        {
            CreateMappingExpression(constructor);
            CreateMemberAssignementForExistingTarget();
        }

        /// <summary>
        /// 为现有目标对象创建成员
        /// </summary>
        public virtual void CreateMemberAssignementForExistingTarget()
        {
            if (PropertiesMapping.Count > 0)
            {
                // 用于更改原始表达式的参数。
                var paramTarget = Expression.Parameter(TargetType, paramClassSource.Name.Replace("s", "t"));
                ChangParameterExpressionVisitor visitSource = new ChangParameterExpressionVisitor(paramClassSource);
                ChangParameterExpressionVisitor visitTarget = new ChangParameterExpressionVisitor(paramTarget);

                List<Expression> finalAssign = new List<Expression>();

                foreach (var item in PropertiesMapping)
                {
                    var propToAssign = visitTarget.Visit(item.Item2);
                    var assignExpression = visitSource.Visit(item.Item1);
                    Type sourceType = TypeSystem.GetElementType(item.Item2.Type);
                    Type targetType = TypeSystem.GetElementType(item.Item1.Type);
                    if (string.IsNullOrEmpty(item.Item4))
                    {
                        object defaultValue = MapperHelper.GetDefaultValue(item.Item2.Type);
                        Expression defaultExpression = Expression.Constant(defaultValue, item.Item2.Type);
                        Expression checkIfNull = Expression.NotEqual(assignExpression, defaultExpression);
                        if (item.Item3)
                        {
                            Expression setIf = Expression.IfThen(checkIfNull, Expression.Assign(propToAssign, assignExpression));
                            finalAssign.Add(setIf);
                        }
                        else
                        {
                            if (!IsListOf(propToAssign.Type))
                            {
                                finalAssign.Add(Expression.Assign(propToAssign, assignExpression));
                            }
                            else
                            {
                                if (sourceType == targetType)
                                {
                                    Expression toListExp = Expression.Call(_toListMethod.MakeGenericMethod(sourceType), assignExpression);
                                    Expression setIf = Expression.IfThen(checkIfNull, Expression.Assign(propToAssign, assignExpression));
                                    finalAssign.Add(setIf);
                                    finalAssign.Add(toListExp);
                                }
                            }
                        }
                    }
                    else // 来自其他映射器。
                    {
                        var mapper = GetMapper(sourceType, targetType, false, item.Item4);
                        if (mapper != null)
                        {
                            mapper.Initialize(_constructorFunc);

                            Expression defaultExpression = Expression.Constant(MapperHelper.GetDefaultValue(item.Item2.Type), item.Item2.Type);
                            if (!IsListOf(propToAssign.Type))
                            {
                                ChangParameterExpressionVisitor changeVisitor = new ChangParameterExpressionVisitor(propToAssign, assignExpression);

                                Expression modifiedExpression = changeVisitor.Visit(mapper.expressionForExisting.Body);
                                Expression checkIfNull = Expression.NotEqual(propToAssign, defaultExpression);
                                Expression setIf = Expression.IfThen(checkIfNull, modifiedExpression);
                                assignExpression = setIf;
                            }
                            else
                            {
                                //Expression selectExp = Expression.Call(_selectMethod.MakeGenericMethod(sourceType), Expression.Constant(mapper.GetDelegate()));
                                Expression checkIfNull = Expression.NotEqual(propToAssign, defaultExpression);
                                Expression setIf = Expression.IfThen(checkIfNull, Expression.Assign(propToAssign, assignExpression));
                                assignExpression = setIf;
                            }

                            finalAssign.Add(assignExpression);
                        }
                    }
                }

                if (finalAssign.Count > 0 && _delegateCallForExisting == null)
                {
                    expressionForExisting = Expression.Lambda(Expression.Block(typeof(void), finalAssign), paramClassSource, paramTarget);
                    // 编译
                    _delegateCallForExisting = expressionForExisting.Compile();
                }
            }
        }

        internal Expression GetLambdaDest(string propertyName)
        {
            var exp = PropertiesMapping.Find(x => GetPropertyInfo(x.Item1).Name == propertyName);
            if (exp != null)
            {
                var final = exp.Item2;
                if (final.NodeType == ExpressionType.Convert)
                {
                    final = (final as UnaryExpression).Operand;
                }

                return final;
            }

            return null;
        }

        /// <summary>
        /// 创建映射表达式树
        /// </summary>
        /// <param name="constructor"></param>
        public virtual void CreateMappingExpression(Func<Type, object> constructor)
        {
            if (!_isInitialized)
            {
                // 它是在处理前放置以避免递归循环。
                _isInitialized = true;
                _constructorFunc = constructor;
                CreateCommonMember();
                var propsToAnalyse = PropertiesMapping.ToList(); // 克隆列表以便于更改。
                for (int i = 0; i < propsToAnalyse.Count; i++)
                {
                    var propToAnalyse = propsToAnalyse[i];
                    CheckAndConfigureMapping(ref propToAnalyse);
                    propsToAnalyse[i] = propToAnalyse;
                }

                PropertiesMapping = propsToAnalyse;
                // 编译
                GetDelegate();
            }
        }

        internal Type GetRealType(Type typeToFind)
        {
            if (UseServiceLocator)
                return _constructorFunc(typeToFind).GetType();
            return typeToFind;
        }

        internal PropertiesNotMapped GetPropertiesNotMapped()
        {
            PropertiesNotMapped result = new PropertiesNotMapped();
            // 克隆属性信息
            List<PropertyInfo> sourceProperties = SourceType.GetProperties().ToList();
            List<PropertyInfo> targetProperties = TargetType.GetProperties().ToList();

            PropertiesVisitor visitor = new PropertiesVisitor(TargetType);
            foreach (var members in memberForNew)
            {
                var members1 = members;
                sourceProperties.RemoveAll((p) => members1.Member.Name == p.Name);
                targetProperties.RemoveAll((p) => visitor.GetProperties(members.Expression).Contains(p));
            }

            // 检查被忽略映射的成员
            sourceProperties.RemoveAll((p) => _propertiesToIgnore.Contains(p));
            result.sourceProperties = sourceProperties;
            result.targetProperties = targetProperties;

            return result;
        }

        /// <summary>
        /// 获取排序表达式树
        /// </summary>
        /// <param name="propertySource">属性名</param>
        /// <returns></returns>
        public LambdaExpression GetSortedExpression(string propertySource)
        {
            var exp = PropertiesMapping.Find(x => GetPropertyInfo(x.Item2).Name == propertySource);
            if (exp == null)
            {
                throw new PropertyNoExistException(propertySource, TargetType);
            }

            // 更改参数
            var visitor = new MapperExpressionVisitor(paramClassSource);
            var result = visitor.Visit(exp.Item1);
            return Expression.Lambda(result, paramClassSource);
        }

        private static bool IsListOf(Type typeTarget)
        {
            // 特殊情况字符串是char数组。
            if (typeTarget == typeof(string))
            {
                return false;
            }

            Func<Type, bool> test = t => t.IsAssignableFrom(typeof(IEnumerable));
            return test(typeTarget) || typeTarget.GetInterfaces().Any(test);
        }

        private MapperConfigurationBase GetAndCheckMapper(Type typeOfSource, Type typeOfTarget, string name)
        {
            var externalMapper = GetMapper(typeOfSource, typeOfTarget, false, name);
            if (externalMapper != null)
            {
                return externalMapper;
            }

            //如果找不到具有别名的映射器
            if (!string.IsNullOrEmpty(name))
            {
                throw new NoFoundMapperException(name);
            }

            throw new NotSameTypePropertyException(typeOfSource, typeOfTarget);
        }

        private void CreatBindingFromSimple(ref Tuple<Expression, Expression, bool, string> configExpression, Type typeSource, Type typeTarget, PropertyInfo propTarget)
        {
            // 没有特殊的操作
            if (typeSource == typeTarget)
            {
                // 创建成员绑定
                CreateMemberBinding(configExpression.Item1, propTarget, configExpression.Item3);
            }
            else
            {
                // 尝试查找mapper
                MapperConfigurationBase externalMapper = GetAndCheckMapper(typeSource, typeTarget, configExpression.Item4);
                // 如果此时未初始化映射器
                externalMapper.CreateMappingExpression(_constructorFunc);
                // 默认情况下，检查对象的null
                Expression mapExpression = externalMapper.GetMemberInitExpression();
                Expression defaultExpression = Expression.Constant(MapperHelper.GetDefaultValue(configExpression.Item1.Type), configExpression.Item1.Type);
                // 修改成员
                Expression expSource = visitorMapper.Visit(configExpression.Item1);
                ChangParameterExpressionVisitor changeParamaterVisitor = new ChangParameterExpressionVisitor(expSource);
                mapExpression = changeParamaterVisitor.Visit(mapExpression);
                // 现在可以创建正确的参数。
                Expression checkIfNull = Expression.NotEqual(expSource, defaultExpression);
                // 创建条件
                var checkExpression = Expression.Condition(checkIfNull, mapExpression, Expression.Constant(MapperHelper.GetDefaultValue(mapExpression.Type), mapExpression.Type), mapExpression.Type);
                MemberAssignment bindExpression = Expression.Bind(propTarget, checkExpression);
                // 找到了映射器但没有配置
                if (string.IsNullOrEmpty(configExpression.Item4))
                {
                    configExpression = Tuple.Create(configExpression.Item1, configExpression.Item2, configExpression.Item3, externalMapper.Name);
                }

                memberForNew.Add(bindExpression);
            }
        }

        private void CreateBindingFromList(ref Tuple<Expression, Expression, bool, string> configExpression, Type typeSource, Type typeTarget, PropertyInfo propTarget)
        {
            Type sourceTypeList = TypeSystem.GetElementType(typeSource);
            Type destTypeList = TypeSystem.GetElementType(typeTarget);
            if (sourceTypeList == destTypeList)
            {
                if (configExpression.Item2.NodeType == ExpressionType.MemberAccess)
                {
                    CreateMemberBinding(configExpression.Item1, propTarget, configExpression.Item3);
                }
            }
            // 使用Enumerable类的select方法来更改类型
            else
            {
                var externalMapper = GetAndCheckMapper(sourceTypeList, destTypeList, configExpression.Item4);
                externalMapper.CreateMappingExpression(_constructorFunc);
                MemberAssignment expBind;
                Expression expSource = configExpression.Item1;

                ChangParameterExpressionVisitor visitor = new ChangParameterExpressionVisitor(paramClassSource);
                expSource = visitor.Visit(expSource);

                // 为了与EF / LINQ2SQL兼容。
                LambdaExpression expMappeur = externalMapper.GetGenericLambdaExpression();
                // 创建对Select方法的调用，在Enumerable的Select中插入一个lambda表达式（参数是一个委托），通常情况下，这是不可能的，但（个人认为）编译器就像这样创建并且LINQ2SQL / EF是可以进行sql查询的
                Expression select = Expression.Call(_selectMethod.MakeGenericMethod(sourceTypeList, destTypeList), new[]
                {
                    expSource,
                    expMappeur
                });
                // 创建对ToList方法的调用
                Expression toList = Expression.Call(_toListMethod.MakeGenericMethod(destTypeList), select);

                if (configExpression.Item3) // 如果要检查无效（使用EF / LinqTosql，则不需要）。
                {
                    // 测试source的属性是否为null。
                    Expression checkIfNull = Expression.NotEqual(expSource, Expression.Constant(MapperHelper.GetDefaultValue(expSource.Type), expSource.Type));
                    // 有时候ToList方法不起作用，则使用实现ToList不起作用的类
                    Expression asExp = Expression.TypeAs(toList, propTarget.PropertyType);
                    // 创建条件表达式
                    Expression expCondition = Expression.Condition(checkIfNull, asExp, Expression.Constant(MapperHelper.GetDefaultValue(typeTarget), typeTarget));

                    // 分配给目标属性。
                    expBind = Expression.Bind(propTarget, expCondition);
                }
                else
                {
                    // 分配给目标属性。
                    expBind = Expression.Bind(propTarget, toList);
                }

                // 查找mapper
                if (string.IsNullOrEmpty(configExpression.Item4))
                {
                    configExpression = Tuple.Create(configExpression.Item1, configExpression.Item2, configExpression.Item3, externalMapper.Name);
                }

                memberForNew.Add(expBind);
            }
        }
    }
}