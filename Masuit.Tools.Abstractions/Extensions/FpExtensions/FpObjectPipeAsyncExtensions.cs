using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Masuit.Tools;

namespace System
{
    public static class FpObjectPipeAsyncExtensions
    {
        /// <summary>
        /// 管道 <para></para>
        /// a->(a->bool)->(a->a) => a <para></para>
        /// 示例： string->(string->bool)->(string->string) => string
        /// </summary>
        /// <Value>
        /// <para><paramref name="input"/>：要处理的值 </para>
        /// <para><paramref name="isExecute"/>：判断是否执行，返回true为执行 </para>
        /// <para><paramref name="func"/>：将要执行的处理 </para>
        /// </Value>
        /// <typeparam name="TInput">可传递任意类型</typeparam>
        /// <returns></returns>
        public static Task<TInput> PipeAsync<TInput>(
            this Task<TInput> input,
             Func<TInput, bool> isExecute,
             Func<TInput, TInput> func
            )
        {
            isExecute.CheckNullWithException(nameof(isExecute));
            func.CheckNullWithException(nameof(func));

            return input.ContinueWith(inObj =>
            {
                if (isExecute(inObj.Result))
                {
                    return func(inObj.Result);
                }
                else
                {
                    return inObj.Result;
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// 管道 <para></para>
        /// a->(a->b) => b <para></para>
        /// 示例： string->(string->int) => int
        /// </summary>
        /// <Value>
        /// <para><paramref name="input"/>：要处理的值 </para>
        /// <para><paramref name="func"/>：将要执行的处理 </para>
        /// </Value>
        /// <typeparam name="TInput">可传递任意类型</typeparam>
        /// <typeparam name="TOutput">输出类型</typeparam>
        /// <returns></returns>
        public static Task<TOutput> PipeAsync<TInput, TOutput>(
            this Task<TInput> input,
             Func<TInput, TOutput> func)
        {
            return input.ContinueWith(t => func(t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}