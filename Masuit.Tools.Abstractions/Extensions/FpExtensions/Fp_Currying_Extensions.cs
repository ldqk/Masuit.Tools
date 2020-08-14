namespace System
{
    /// <summary>
    /// 函数式扩展-柯里化
    /// </summary>
    public static class Fp_Currying_Extensions
    {
        #region Action

        /// <summary>
        /// 柯里化<para></para>
        /// (a->void) => (a->(()->void)) <para></para>
        /// 示例： (int->void) => (int->(()->void))
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Func<T1, Action> Currying<T1>(
            this Action<T1> action)
        {
            return x => () => action(x);
        }

        /// <summary>
        /// 柯里化<para></para>
        /// (a->b->void) => (a->(b->(()->void))) <para></para>
        /// 示例： (int->string->void) => (int->(string->(()->void)))
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Func<T1, Func<T2, Action>> Currying<T1, T2>(
            this Action<T1, T2> action)
        {
            return x => y => () => action(x, y);
        }

        public static Func<T1, Func<T2, Func<T3, Action>>> Currying<T1, T2, T3>(
            this Action<T1, T2, T3> action)
        {
            return x => y => z => () => action(x, y, z);
        }

        public static Func<T1, Func<T2, Func<T3, Func<T4, Action>>>> Currying<T1, T2, T3, T4>(
            this Action<T1, T2, T3, T4> action)
        {
            return x => y => z => g => () => action(x, y, z, g);
        }

        #endregion Action

        #region Func

        //Func<SensorMessage,Action<byte[]> > -> Currying -> Func<SensorMessage, Func<Action<byte[]>>>
        //Func<SensorMessage, Func<Action<byte[]>>>(XXX)
        //Func<Action<byte[]>()
        //Action<byte[]>
        public static Func<T1, Func<TResult>> Currying<T1, TResult>(
            this Func<T1, TResult> func)
        {
            return x => () => func(x);
        }

        public static Func<T1, Func<T2, Func<TResult>>> Currying<T1, T2, TResult>(
            this Func<T1, T2, TResult> func)
        {
            return x => y => () => func(x, y);
        }

        public static Func<T1, Func<T2, Func<T3, Func<TResult>>>> Currying<T1, T2, T3, TResult>(
            this Func<T1, T2, T3, TResult> func)
        {
            return x => y => z => () => func(x, y, z);
        }

        public static Func<T1, Func<T2, Func<T3, Func<T4, Func<TResult>>>>> Currying<T1, T2, T3, T4, TResult>(
            this Func<T1, T2, T3, T4, TResult> func)
        {
            return x => y => z => g => () => func(x, y, z, g);
        }

        #endregion Func

        //todo: 后续还有东西没理解：https://zhuanlan.zhihu.com/p/94591842
    }
}