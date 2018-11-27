using System.Collections.Generic;

namespace Masuit.Tools.Systems
{
    /// <summary>
    /// 定长队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LimitedQueue<T> : Queue<T>
    {
        public int Limit { get; set; }

        public LimitedQueue(int limit) : base(limit)
        {
            Limit = limit;
        }

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