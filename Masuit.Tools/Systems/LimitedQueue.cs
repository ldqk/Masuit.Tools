using System.Collections.Generic;

namespace Masuit.Tools.Systems
{
    /// <summary>
    /// 定长队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LimitedQueue<T> : Queue<T>
    {
        /// <summary>
        /// 队列长度
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// 定长队列
        /// </summary>
        /// <param name="limit"></param>
        public LimitedQueue(int limit) : base(limit)
        {
            Limit = limit;
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="item"></param>
        public new void Enqueue(T item)
        {
            if (Count >= Limit)
            {
                Dequeue();
            }

            base.Enqueue(item);
        }
    }
}