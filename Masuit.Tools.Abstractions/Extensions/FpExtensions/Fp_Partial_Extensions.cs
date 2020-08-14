namespace System
{
    /// <summary>
    /// 函数式扩展-分布函数
    /// </summary>
    public static class Fp_Partial_Extensions
    {
        #region Action

        public static Action<T1> Partial<T1>(
            this Func<T1, Action> func)
        {
            return x => func(x);
        }

        public static Action<T1, T2> Partial<T1, T2>(
            this Func<T1, Func<T2, Action>> func)
        {
            return (x, y) => func(x)(y);
        }

        public static Action<T1, T2, T3> Partial<T1, T2, T3>(
            this Func<T1, Func<T2, Func<T3, Action>>> func)
        {
            return (x, y, z) => func(x)(y)(z);
        }

        public static Action<T1, T2, T3, T4> Partial<T1, T2, T3, T4>(
            this Func<T1, Func<T2, Func<T3, Func<T4, Action>>>> func)
        {
            return (x, y, z, g) => func(x)(y)(z)(g);
        }

        #endregion Action

        #region Func

        public static Func<T1, TResult> Partial<T1, TResult>(
            this Func<T1, Func<TResult>> func)
        {
            return x => func(x)();
        }

        public static Func<T1, T2, TResult> Partial<T1, T2, TResult>(
            this Func<T1, Func<T2, Func<TResult>>> func)
        {
            return (x, y) => func(x)(y)();
        }

        public static Func<T1, T2, T3, TResult> Partial<T1, T2, T3, TResult>(
            this Func<T1, Func<T2, Func<T3, Func<TResult>>>> func)
        {
            return (x, y, z) => func(x)(y)(z)();
        }

        public static Func<T1, T2, T3, T4, TResult> Partial<T1, T2, T3, T4, TResult>(
            this Func<T1, Func<T2, Func<T3, Func<T4, Func<TResult>>>>> func)
        {
            return (x, y, z, g) => func(x)(y)(z)(g)();
        }

        #endregion Func
    }
}