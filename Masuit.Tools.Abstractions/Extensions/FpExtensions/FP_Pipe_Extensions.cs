using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Masuit.Tools;

namespace System
{
    /// <summary>
    /// 函数式扩展-管道
    /// </summary>
    public static class FP_Pipe_Extensions
    {
        #region 返回Action

        #region 1个入参

        /// <summary>
        /// 管道 <para></para>
        /// (a->void)->(a->void)->...  => (a->void) <para></para>
        /// 示例:  (string->void)->(string->void)->...  => (string->void)
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="sourceFunc"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static Action<TInput> Pipe<TInput>(
             this Action<TInput> sourceFunc,
             params Action<TInput>[] actions)
        {
            sourceFunc.CheckNullWithException(nameof(sourceFunc));
            actions.CheckNullWithException(nameof(actions));

            return t =>
            {
                sourceFunc(t);
                actions.ForEach(item => item(t));
            };
        }

        /// <summary>
        /// 管道 <para></para>
        /// (a->b)->(b->void)->...  => (a->void) <para></para>
        /// 示例:  (string->int)->(int->void)->...  => (string->void)
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sourceFunc"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static Action<TInput> Pipe<TInput, TResult>(
             this Func<TInput, TResult> sourceFunc,
             params Action<TResult>[] actions)
        {
            sourceFunc.CheckNullWithException(nameof(sourceFunc));
            actions.CheckNullWithException(nameof(actions));

            return t =>
            {
                TResult tempResult = sourceFunc(t);
                actions.ForEach(item => item(tempResult));
            };
        }

        #endregion 1个入参

        #endregion 返回Action

        #region Func

        /// <summary>
        /// 管道 <para></para>
        /// (a->b)->(b->c) => (a->c) <para></para>
        /// 示例： (string->int)->(int->bool) => (string->bool)
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TCenter"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sourceFunc"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Func<TInput, TResult> Pipe<TInput, TCenter, TResult>(
           this Func<TInput, TCenter> sourceFunc,
           Func<TCenter, TResult> func)
        {
            sourceFunc.CheckNullWithException(nameof(sourceFunc));
            func.CheckNullWithException(nameof(func));

            return t => func(sourceFunc(t));
        }

        #endregion Func
    }
}