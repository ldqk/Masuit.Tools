using Masuit.Tools.Mapping.Core;
using Masuit.Tools.Mapping.Exceptions;
using System;
using System.Linq.Expressions;

namespace Masuit.Tools.Mapping
{
    /// <summary>
    /// mapper的基类
    /// </summary>
    public static class ExpressionMapper
    {
        private static Func<Type, object> _constructorFunc;
        private static bool _initialized;

        /// <summary>
        /// 映射指定的源。
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TDest">目标类型</typeparam>
        /// <param name="source">源对象</param>
        /// <param name="name">别名</param>
        /// <returns>
        /// 目标对象的新实例
        /// </returns>
        public static TDest Map<TSource, TDest>(this TSource source, string name = null) where TSource : class where TDest : class
        {
            if (source == null)
            {
                return null;
            }

            if (!_initialized)
            {
                Initialize();
            }
            TDest result = null;
            MapperConfiguration<TSource, TDest> mapper = GetMapper<TSource, TDest>(name);
            Func<TSource, TDest> query = mapper.GetFuncDelegate();
            if (query != null)
            {
                result = query(source);
                // 映射后执行的操作
                mapper.ExecuteAfterActions(source, result);
            }
            return result;
        }

        /// <summary>
        /// 将指定的源映射到目标。
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TDest">目标类型</typeparam>
        /// <param name="source">源对象</param>
        /// <param name="target">目标对象</param>
        /// <param name="name">别名</param>
        public static void Map<TSource, TDest>(this TSource source, TDest target, string name = null) where TSource : class where TDest : class
        {
            if (!_initialized)
            {
                Initialize();
            }
            TDest result = null;
            MapperConfiguration<TSource, TDest> mapper = GetMapper<TSource, TDest>(name);
            Action<TSource, TDest> query = mapper.GetDelegateForExistingTarget() as Action<TSource, TDest>;
            if (query != null)
            {
                query(source, target);
                // 映射后执行的操作
                mapper.ExecuteAfterActions(source, result);
            }
        }

        /// <summary>
        /// 获取查询表达式树
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TDest">目标类型</typeparam>
        /// <returns></returns>
        public static Expression<Func<TSource, TDest>> GetQueryExpression<TSource, TDest>() where TSource : class where TDest : class
        {
            return GetMapper<TSource, TDest>().GetLambdaExpression();
        }

        /// <summary>
        /// 创建mapper对象
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TDest">目标类型</typeparam>
        /// <returns></returns>
        public static MapperConfiguration<TSource, TDest> CreateMap<TSource, TDest>(string name = null) where TSource : class where TDest : class
        {
            MapperConfigurationBase map = MapperConfigurationCollectionContainer.Instance.Find(typeof(TSource), typeof(TDest), name);
            if (map == null)
            {
                string finalName = string.IsNullOrEmpty(name) ? "s" + MapperConfigurationCollectionContainer.Instance.Count.ToString() : name;
                map = new MapperConfiguration<TSource, TDest>(finalName);
                MapperConfigurationCollectionContainer.Instance.Add(map);
            }
            return map as MapperConfiguration<TSource, TDest>;
        }

        /// <summary>
        /// 表示使用的依赖注入服务
        /// </summary>
        /// <param name="constructor">构造函数委托</param>
        public static void ConstructServicesUsing(Func<Type, object> constructor)
        {
            _constructorFunc = constructor;
        }

        /// <summary>
        /// 重置mapper
        /// </summary>
        public static void Reset()
        {
            MapperConfigurationCollectionContainer.Instance.Clear();
        }

        /// <summary>
        /// 获取mapper实例
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TDest">目标类型</typeparam>
        /// <param name="name">别名</param>
        /// <returns></returns>
        public static MapperConfiguration<TSource, TDest> GetMapper<TSource, TDest>(string name = null) where TSource : class where TDest : class
        {
            return GetMapper(typeof(TSource), typeof(TDest), name) as MapperConfiguration<TSource, TDest>;
        }

        /// <summary>
        /// 初始化mapper
        /// </summary>
        public static void Initialize()
        {
            MapperConfigurationCollectionContainer configRegister = MapperConfigurationCollectionContainer.Instance;
            foreach (var t in configRegister)
            {
                t.Initialize(_constructorFunc);
            }
            _initialized = true;
        }

        /// <summary>
        /// 获取mapper的委托
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TDest">目标类型</typeparam>
        /// <returns></returns>
        public static Func<TSource, TDest> GetQuery<TSource, TDest>() where TSource : class where TDest : class
        {
            return GetMapper<TSource, TDest>().GetFuncDelegate();
        }

        /// <summary>
        /// 获取未映射的属性
        /// </summary>
        public static PropertiesNotMapped GetPropertiesNotMapped<TSource, TDest>(string name = null) where TSource : class where TDest : class
        {
            var mapper = GetMapper<TSource, TDest>(name);
            return mapper.GetPropertiesNotMapped();
        }

        internal static MapperConfigurationBase GetMapper(Type tSource, Type tDest, string name = null)
        {
            MapperConfigurationBase mapConfig = MapperConfigurationCollectionContainer.Instance.Find(tSource, tDest, name);
            if (mapConfig == null)
            {
                throw new NoFoundMapperException(tSource, tDest);
            }

            return mapConfig;
        }
    }
}