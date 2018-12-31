using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Masuit.Tools
{
    internal static class ExpressionGenericMapper<TSource, TDest> where TSource : class where TDest : class
    {

        // 缓存委托
        private static Func<TSource, TDest> MapFunc { get; set; }
        private static Action<TSource, TDest> MapAction { get; set; }
        private static Func<TSource, TDest> _copyFunc;
        private static Action<TSource, TDest> _copyAction;

        /// <summary>
        /// 将对象TSource转换为TDest
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TDest Map(TSource source)
        {
            if (MapFunc == null)
            {
                MapFunc = GetMapFunc();
            }

            return MapFunc(source);
        }

        /// <summary>
        /// 集合元素映射
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        public static List<TDest> MapList(IEnumerable<TSource> sources)
        {
            if (MapFunc == null)
            {
                MapFunc = GetMapFunc();
            }

            var result = new List<TDest>();
            foreach (var item in sources)
            {
                result.Add(MapFunc(item));
            }

            return result;
        }

        /// <summary>
        /// 将对象TSource的值赋给给TDest
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void Map(TSource source, TDest target)
        {
            if (MapAction == null)
            {
                MapAction = GetMapAction();
            }

            MapAction(source, target);
        }

        private static Func<TSource, TDest> GetMapFunc()
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TDest);
            //Func委托传入变量
            var parameter = Expression.Parameter(sourceType, "p");

            var memberBindings = new List<MemberBinding>();
            var targetTypes = targetType.GetProperties().Where(x => x.PropertyType.IsPublic && x.CanWrite);
            foreach (var targetItem in targetTypes)
            {
                var sourceItem = sourceType.GetProperty(targetItem.Name);

                //判断实体的读写权限
                if (sourceItem == null || !sourceItem.CanRead || sourceItem.PropertyType.IsNotPublic)
                {
                    continue;
                }

                //标注NotMapped特性的属性忽略转换
                if (sourceItem.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue;
                }

                var sourceProperty = Expression.Property(parameter, sourceItem);

                //当非值类型且类型不相同时
                if (!sourceItem.PropertyType.IsValueType && sourceItem.PropertyType != targetItem.PropertyType)
                {
                    //判断都是(非泛型)class
                    if (sourceItem.PropertyType.IsClass && targetItem.PropertyType.IsClass && !sourceItem.PropertyType.IsGenericType && !targetItem.PropertyType.IsGenericType)
                    {
                        var expression = GetClassExpression(sourceProperty, sourceItem.PropertyType, targetItem.PropertyType);
                        memberBindings.Add(Expression.Bind(targetItem, expression));
                    }

                    //集合数组类型的转换
                    if (typeof(IEnumerable).IsAssignableFrom(sourceItem.PropertyType) && typeof(IEnumerable).IsAssignableFrom(targetItem.PropertyType))
                    {
                        var expression = GetListExpression(sourceProperty, sourceItem.PropertyType, targetItem.PropertyType);
                        memberBindings.Add(Expression.Bind(targetItem, expression));
                    }

                    continue;
                }

                if (targetItem.PropertyType != sourceItem.PropertyType)
                {
                    continue;
                }

                memberBindings.Add(Expression.Bind(targetItem, sourceProperty));
            }

            //创建一个if条件表达式
            var test = Expression.NotEqual(parameter, Expression.Constant(null, sourceType)); // p==null;
            var ifTrue = Expression.MemberInit(Expression.New(targetType), memberBindings);
            var condition = Expression.Condition(test, ifTrue, Expression.Constant(null, targetType));

            var lambda = Expression.Lambda<Func<TSource, TDest>>(condition, parameter);
            return lambda.Compile();
        }

        /// <summary>
        /// 类型是clas时赋值
        /// </summary>
        /// <param name="sourceProperty"></param>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        private static Expression GetClassExpression(Expression sourceProperty, Type sourceType, Type targetType)
        {
            //条件p.Item!=null    
            var testItem = Expression.NotEqual(sourceProperty, Expression.Constant(null, sourceType));

            //构造回调 Mapper<TSource, TDest>.Map()
            var mapperType = typeof(ExpressionGenericMapper<,>).MakeGenericType(sourceType, targetType);
            var iftrue = Expression.Call(mapperType.GetMethod(nameof(Map), new[]
            {
                sourceType
            }), sourceProperty);
            var conditionItem = Expression.Condition(testItem, iftrue, Expression.Constant(null, targetType));
            return conditionItem;
        }

        /// <summary>
        /// 类型为集合时赋值
        /// </summary>
        /// <param name="sourceProperty"></param>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        private static Expression GetListExpression(Expression sourceProperty, Type sourceType, Type targetType)
        {
            //条件p.Item!=null    
            var testItem = Expression.NotEqual(sourceProperty, Expression.Constant(null, sourceType));

            //构造回调
            var sourceArg = sourceType.IsArray ? sourceType.GetElementType() : sourceType.GetGenericArguments()[0];
            var targetArg = targetType.IsArray ? targetType.GetElementType() : targetType.GetGenericArguments()[0];
            var mapperType = typeof(ExpressionGenericMapper<,>).MakeGenericType(sourceArg, targetArg);

            var mapperExecMap = Expression.Call(mapperType.GetMethod(nameof(MapList), new[]
            {
                sourceType
            }), sourceProperty);

            Expression iftrue;
            if (targetType == mapperExecMap.Type)
            {
                iftrue = mapperExecMap;
            }
            else if (targetType.IsArray) //数组类型调用ToArray()方法
            {
                iftrue = Expression.Call(mapperExecMap, mapperExecMap.Type.GetMethod("ToArray"));
            }
            else if (typeof(IDictionary).IsAssignableFrom(targetType))
            {
                iftrue = Expression.Constant(null, targetType); //字典类型不转换
            }
            else
            {
                iftrue = Expression.Convert(mapperExecMap, targetType);
            }

            var conditionItem = Expression.Condition(testItem, iftrue, Expression.Constant(null, targetType));
            return conditionItem;
        }

        private static Action<TSource, TDest> GetMapAction()
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TDest);
            //Func委托传入变量
            var sourceParameter = Expression.Parameter(sourceType, "p");
            var targetParameter = Expression.Parameter(targetType, "t");

            //创建一个表达式集合
            var expressions = new List<Expression>();
            var targetTypes = targetType.GetProperties().Where(x => x.PropertyType.IsPublic && x.CanWrite);
            foreach (var targetItem in targetTypes)
            {
                var sourceItem = sourceType.GetProperty(targetItem.Name);

                //判断实体的读写权限
                if (sourceItem == null || !sourceItem.CanRead || sourceItem.PropertyType.IsNotPublic)
                {
                    continue;
                }

                //标注NotMapped特性的属性忽略转换
                if (sourceItem.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue;
                }

                var sourceProperty = Expression.Property(sourceParameter, sourceItem);
                var targetProperty = Expression.Property(targetParameter, targetItem);

                //当非值类型且类型不相同时
                if (!sourceItem.PropertyType.IsValueType && sourceItem.PropertyType != targetItem.PropertyType)
                {
                    //判断都是(非泛型)class
                    if (sourceItem.PropertyType.IsClass && targetItem.PropertyType.IsClass && !sourceItem.PropertyType.IsGenericType && !targetItem.PropertyType.IsGenericType)
                    {
                        var expression = GetClassExpression(sourceProperty, sourceItem.PropertyType, targetItem.PropertyType);
                        expressions.Add(Expression.Assign(targetProperty, expression));
                    }

                    //集合数组类型的转换
                    if (typeof(IEnumerable).IsAssignableFrom(sourceItem.PropertyType) && typeof(IEnumerable).IsAssignableFrom(targetItem.PropertyType))
                    {
                        var expression = GetListExpression(sourceProperty, sourceItem.PropertyType, targetItem.PropertyType);
                        expressions.Add(Expression.Assign(targetProperty, expression));
                    }

                    continue;
                }

                if (targetItem.PropertyType != sourceItem.PropertyType)
                {
                    continue;
                }

                expressions.Add(Expression.Assign(targetProperty, sourceProperty));
            }

            //当Target!=null判断source是否为空
            var testSource = Expression.NotEqual(sourceParameter, Expression.Constant(null, sourceType));
            var ifTrueSource = Expression.Block(expressions);
            var conditionSource = Expression.IfThen(testSource, ifTrueSource);

            //判断target是否为空
            var tesTDest = Expression.NotEqual(targetParameter, Expression.Constant(null, targetType));
            var conditionTarget = Expression.IfThen(tesTDest, conditionSource);
            var lambda = Expression.Lambda<Action<TSource, TDest>>(conditionTarget, sourceParameter, targetParameter);
            return lambda.Compile();
        }

        /// <summary>
        /// 新建目标类型实例，并将源对象的属性值拷贝至目标对象的对应属性
        /// </summary>
        /// <param name="source">源对象实例</param>
        /// <returns>深拷贝了源对象属性的目标对象实例</returns>
        public static TDest Copy(TSource source)
        {
            if (source == null) return default(TDest);

            // 因为对于泛型类型而言，每次传入不同的泛型参数都会调用静态构造函数，所以可以通过这种方式进行缓存
            if (_copyFunc != null)
            {
                // 如果之前缓存过，则直接调用缓存的委托
                return _copyFunc(source);
            }

            Type sourceType = typeof(TSource);
            Type targetType = typeof(TDest);

            var paramExpr = Expression.Parameter(sourceType, nameof(source));

            Expression bodyExpr;

            // 如果对象可以遍历（目前只支持数组和ICollection<T>实现类）
            if (sourceType == targetType && MapperTools.IsIEnumerableExceptString(sourceType))
            {
                bodyExpr = Expression.Call(null, EnumerableCopier.GetMethondInfo(sourceType), paramExpr);
            }
            else
            {
                var memberBindings = new List<MemberBinding>();
                // 遍历目标对象的所有属性信息
                foreach (var targetPropInfo in targetType.GetProperties())
                {
                    // 从源对象获取同名的属性信息
                    var sourcePropInfo = sourceType.GetProperty(targetPropInfo.Name);
                    if (sourcePropInfo is null)
                    {
                        continue;
                    }

                    Type sourcePropType = sourcePropInfo?.PropertyType;
                    Type targetPropType = targetPropInfo.PropertyType;

                    // 只在满足以下三个条件的情况下进行拷贝
                    // 1.源属性类型和目标属性类型一致
                    // 2.源属性可读
                    // 3.目标属性可写
                    if (sourcePropInfo.CanRead && targetPropInfo.CanWrite)
                    {
                        // 获取属性值的表达式
                        Expression expression = Expression.Property(paramExpr, sourcePropInfo);

                        // 如果目标属性是值类型或者字符串，则直接做赋值处理
                        // 暂不考虑目标值类型有非字符串的引用类型这种特殊情况
                        // 非字符串引用类型做递归处理
                        if (MapperTools.IsRefTypeExceptString(targetPropType))
                        {
                            // 进行递归
                            if (MapperTools.IsRefTypeExceptString(targetPropType))
                            {
                                expression = Expression.Call(null, GetCopyMethodInfo(sourcePropType, targetPropType), expression);
                            }
                        }

                        memberBindings.Add(Expression.Bind(targetPropInfo, expression));
                    }
                }

                bodyExpr = Expression.MemberInit(Expression.New(targetType), memberBindings);
            }

            var lambdaExpr = Expression.Lambda<Func<TSource, TDest>>(bodyExpr, paramExpr);

            _copyFunc = lambdaExpr.Compile();
            return _copyFunc(source);
        }

        /// <summary>
        /// 新建目标类型实例，并将源对象的属性值拷贝至目标对象的对应属性
        /// </summary>
        /// <param name="source">源对象实例</param>
        /// <param name="target">目标对象实例</param>
        public static void Copy(TSource source, TDest target)
        {
            if (source == null) return;

            // 因为对于泛型类型而言，每次传入不同的泛型参数都会调用静态构造函数，所以可以通过这种方式进行缓存
            // 如果之前缓存过，则直接调用缓存的委托
            if (_copyAction != null)
            {
                _copyAction(source, target);
                return;
            }

            Type sourceType = typeof(TSource);
            Type targetType = typeof(TDest);

            // 如果双方都可以被遍历
            if (MapperTools.IsIEnumerableExceptString(sourceType) && MapperTools.IsIEnumerableExceptString(targetType))
            {
                // TODO
                // 向已存在的数组或者ICollection<T>拷贝的功能暂不支持
            }
            else
            {
                var paramSourceExpr = Expression.Parameter(sourceType, nameof(source));
                var paramTargetExpr = Expression.Parameter(targetType, nameof(target));

                var binaryExpressions = new List<Expression>();
                // 遍历目标对象的所有属性信息
                foreach (var targetPropInfo in targetType.GetProperties())
                {
                    // 从源对象获取同名的属性信息
                    var sourcePropInfo = sourceType.GetProperty(targetPropInfo.Name);

                    Type sourcePropType = sourcePropInfo?.PropertyType;
                    Type targetPropType = targetPropInfo.PropertyType;

                    // 只在满足以下三个条件的情况下进行拷贝
                    // 1.源属性类型和目标属性类型一致
                    // 2.源属性可读
                    // 3.目标属性可写
                    if (sourcePropType == targetPropType && sourcePropInfo.CanRead && targetPropInfo.CanWrite)
                    {
                        // 获取属性值的表达式
                        Expression expression = Expression.Property(paramSourceExpr, sourcePropInfo);
                        Expression targetPropExpr = Expression.Property(paramTargetExpr, targetPropInfo);

                        // 如果目标属性是值类型或者字符串，则直接做赋值处理
                        // 暂不考虑目标值类型有非字符串的引用类型这种特殊情况
                        if (MapperTools.IsRefTypeExceptString(targetPropType))
                        {
                            expression = Expression.Call(null, GetCopyMethodInfo(sourcePropType, targetPropType), expression);
                        }

                        binaryExpressions.Add(Expression.Assign(targetPropExpr, expression));
                    }
                }

                Expression bodyExpr = Expression.Block(binaryExpressions);

                var lambdaExpr = Expression.Lambda<Action<TSource, TDest>>(bodyExpr, paramSourceExpr, paramTargetExpr);

                _copyAction = lambdaExpr.Compile();
                _copyAction(source, target);
            }
        }

        private static MethodInfo GetCopyMethodInfo(Type source, Type target) => typeof(ExpressionGenericMapper<,>).MakeGenericType(source, target).GetMethod(nameof(Copy), new[]
        {
            source
        });
    }
}