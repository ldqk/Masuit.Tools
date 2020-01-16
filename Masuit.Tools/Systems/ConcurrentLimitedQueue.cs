using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.Tools.Systems
{
    /// <summary>
    /// 定长队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentLimitedQueue<T> : ConcurrentQueue<T>
    {
        /// <summary>
        /// 长度
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// 定长队列
        /// </summary>
        /// <param name="limit"></param>
        public ConcurrentLimitedQueue(int limit)
        {
            Limit = limit;
        }

        /// <summary>
        /// 定长队列
        /// </summary>
        /// <param name="list"></param>
        public ConcurrentLimitedQueue(IEnumerable<T> list) : base(list)
        {
            Limit = list.Count();
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="item"></param>
        public new void Enqueue(T item)
        {
            if (Count >= Limit)
            {
                TryDequeue(out _);
            }

            base.Enqueue(item);
        }
    }
}