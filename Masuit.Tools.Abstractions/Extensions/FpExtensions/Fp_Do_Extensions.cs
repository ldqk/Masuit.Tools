using System.Diagnostics.CodeAnalysis;

namespace System
{
    public static class Fp_Do_Extensions
    {
        #region 1个参数

        #region Action

        /// <summary>
        /// 管道 <para></para>
        /// 适合： (a->void)->(a->void) => (a->void) <para></para>
        /// 示例:  (string->void)->(string->void) => (string->void)
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="sourceFunc"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Action<TInput> Do<TInput>(
            [NotNull] this Action<TInput> sourceFunc,
            [NotNull] Action<TInput> action)
        {
            sourceFunc.CheckNullWithException(nameof(sourceFunc));
            action.CheckNullWithException(nameof(action));
            sourceFunc += action;
            return sourceFunc;
        }

        /// <summary>
        /// 管道 <para></para>
        /// 适合： (a->void)->(a->void)->...  => (a->void) <para></para>
        /// 示例:  (string->void)->(string->void)->...  => (string->void)
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="sourceFunc"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static Action<TInput> Do<TInput>(
            [NotNull] this Action<TInput> sourceFunc,
            [NotNull] params Action<TInput>[] actions)
        {
            sourceFunc.CheckNullWithException(nameof(sourceFunc));
            actions.CheckNullWithException(nameof(actions));
            actions.For(t => sourceFunc += t);
            return sourceFunc;
        }

        #endregion Action

        #region Func

        /// <summary>
        /// 管道 <para></para>
        /// (a->b)->(b->void) => (a->b) <para></para>
        /// 示例： (string->int)->(int->void) => (string->int)
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sourceFunc"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Func<TInput, TResult> Do<TInput, TResult>(
            [NotNull] this Func<TInput, TResult> sourceFunc,
            [NotNull] Action<TResult> action)
        {
            sourceFunc.CheckNullWithException(nameof(sourceFunc));
            action.CheckNullWithException(nameof(action));

            return t =>
            {
                TResult result = sourceFunc(t);
                action(result);
                return result;
            };
        }

        /// <summary>
        /// 管道 <para></para>
        /// (a->b)->(b->void)->... => (a->b) <para></para>
        /// 示例： (string->int)->(int->void)->... => (string->int)
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sourceFunc"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static Func<TInput, TResult> Do<TInput, TResult>(
            [NotNull] this Func<TInput, TResult> sourceFunc,
            [NotNull] params Action<TResult>[] actions)
        {
            sourceFunc.CheckNullWithException(nameof(sourceFunc));
            actions.CheckNullWithException(nameof(actions));

            return t =>
            {
                TResult result = sourceFunc(t);
                actions.For(item => item(result));
                return result;
            };
        }

        #endregion Func

        #endregion 1个参数
    }
}