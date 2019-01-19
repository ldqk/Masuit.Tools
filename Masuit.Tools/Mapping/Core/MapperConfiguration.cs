using Masuit.Tools.Mapping.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Masuit.Tools.Mapping.Core
{
    /// <summary>
    /// 主映射器
    /// </summary>
    /// <typeparam name="TSource">源类型</typeparam>
    /// <typeparam name="TDest">目标类型</typeparam>
    public class MapperConfiguration<TSource, TDest> : MapperConfigurationBase
    {
        /// <summary>
        /// 
        /// </summary>
        protected readonly IList<Action<TSource, TDest>> actionsAfterMap;

        /// <summary>
        /// 构造函数
        /// </summary>
        public MapperConfiguration(string paramName, string mapperName = null) : base(typeof(TSource), typeof(TDest), paramName, mapperName)
        {
            actionsAfterMap = new List<Action<TSource, TDest>>();
        }

        /// <summary>
        /// 获取Lambda表达式树
        /// </summary>
        /// <returns></returns>
        public Expression<Func<TSource, TDest>> GetLambdaExpression()
        {
            MemberInitExpression exp = GetMemberInitExpression();
            return Expression.Lambda<Func<TSource, TDest>>(exp, paramClassSource);
        }

        /// <summary>
        /// 获取委托
        /// </summary>
        /// <returns></returns>
        public Func<TSource, TDest> GetFuncDelegate()
        {
            return (Func<TSource, TDest>)GetDelegate();
        }

        /// <summary>
        /// 映射成员
        /// </summary>
        /// <param name="getPropertySource">源类型</param>
        /// <param name="getPropertyDest">目标类型</param>
        /// <returns></returns>
        public MapperConfiguration<TSource, TDest> ForMember<TPropertySource, TPropertyDest>(Expression<Func<TSource, TPropertySource>> getPropertySource, Expression<Func<TDest, TPropertyDest>> getPropertyDest)
        {
            // 添加到映射列表并且可以继续操作
            ForMemberBase(getPropertySource.Body, getPropertyDest.Body, false);
            return this;
        }

        /// <summary>
        /// 映射成员
        /// </summary>
        /// <typeparam name="TPropertySource">属性源类型</typeparam>
        /// <typeparam name="TPropertyDest">属性目标类型</typeparam>
        /// <param name="getPropertySource">源类型</param>
        /// <param name="getPropertyDest">目标类型</param>
        /// <param name="checkIfNull">是否检查null值</param>
        /// <returns></returns>
        public MapperConfiguration<TSource, TDest> ForMember<TPropertySource, TPropertyDest>(Expression<Func<TSource, TPropertySource>> getPropertySource, Expression<Func<TDest, TPropertyDest>> getPropertyDest, bool checkIfNull)
        {
            // 添加到映射列表并且可以继续操作
            ForMemberBase(getPropertySource.Body, getPropertyDest.Body, checkIfNull);
            return this;
        }

        /// <summary>
        /// 映射成员
        /// </summary>
        /// <typeparam name="TPropertySource">属性源类型</typeparam>
        /// <typeparam name="TPropertyDest">属性目标类型</typeparam>
        /// <param name="getPropertySource">源类型</param>
        /// <param name="getPropertyDest">目标类型</param>
        /// <param name="mapperName">mapper别名</param>
        /// <returns></returns>
        public MapperConfiguration<TSource, TDest> ForMember<TPropertySource, TPropertyDest>(Expression<Func<TSource, TPropertySource>> getPropertySource, Expression<Func<TDest, TPropertyDest>> getPropertyDest, string mapperName)
        {
            // 添加到映射列表并且可以继续操作
            ForMemberBase(getPropertySource.Body, getPropertyDest.Body, true, mapperName);
            return this;
        }

        /// <summary>
        /// 忽略一些不需要映射的成员
        /// </summary>
        /// <param name="propertyDest">属性名</param>
        /// <returns></returns>
        public MapperConfiguration<TSource, TDest> Ignore<TProperty>(Expression<Func<TDest, TProperty>> propertyDest)
        {
            return IgnoreBase(propertyDest) as MapperConfiguration<TSource, TDest>;
        }

        /// <summary>
        /// 映射后要执行的操作
        /// </summary>
        /// <param name="actionAfterMap">映射后要执行的操作</param>
        /// <returns></returns>
        public MapperConfiguration<TSource, TDest> AfterMap(Action<TSource, TDest> actionAfterMap)
        {
            // 添加到映射列表并且可以继续操作
            actionsAfterMap.Add(actionAfterMap);
            return this;
        }

        /// <summary>
        /// 执行后续操作。
        /// </summary>
        /// <param name="source">源类型</param>
        /// <param name="dest">目标类型</param>
        public void ExecuteAfterActions(TSource source, TDest dest)
        {
            if (actionsAfterMap.Count > 0)
            {
                foreach (var action in actionsAfterMap)
                {
                    if (action == null)
                    {
                        throw new NoActionAfterMappingException();
                    }
                    action(source, dest);
                }
            }
        }

        /// <summary>
        /// 反向映射
        /// </summary>
        /// <param name="name">mapper别名</param>
        /// <returns>
        /// 新的mapper对象
        /// </returns>
        /// <exception cref="MapperExistException"></exception>
        public MapperConfiguration<TDest, TSource> ReverseMap(string name = null)
        {
            MapperConfigurationBase map = GetMapper(typeof(TDest), typeof(TSource), false, name);

            if (map != null)
            {
                throw new MapperExistException(typeof(TDest), typeof(TSource));
            }
            string finalName = string.IsNullOrEmpty(name) ? "s" + (MapperConfigurationCollectionContainer.Instance.Count).ToString() : name;
            map = new MapperConfiguration<TDest, TSource>(finalName);
            MapperConfigurationCollectionContainer.Instance.Add(map);
            CreateCommonMember();

            // 现有属性的映射，并且创建反向关系
            foreach (var item in PropertiesMapping)
            {
                PropertyInfo propertyDest = GetPropertyInfo(item.Item1);
                if (propertyDest.CanWrite)
                {
                    if (!string.IsNullOrEmpty(item.Item4))
                    {
                        //找到反向关系的mapper
                        var reverseMapper = GetMapper(item.Item2.Type, item.Item1.Type, false);
                        if (reverseMapper != null)
                        {
                            map.ForMemberBase(item.Item2, item.Item1, item.Item3, reverseMapper.Name);
                        }
                    }
                    else
                    {
                        if (item.Item1.NodeType == ExpressionType.MemberAccess)
                        {
                            map.ForMemberBase(item.Item2, item.Item1, item.Item3, item.Item4);
                        }
                    }
                }
            }

            return (MapperConfiguration<TDest, TSource>)map;
        }

        /// <summary>
        /// 是否使用服务注入
        /// </summary>
        public MapperConfiguration<TSource, TDest> ConstructUsingServiceLocator()
        {
            UseServiceLocator = true;
            return this;
        }
    }
}