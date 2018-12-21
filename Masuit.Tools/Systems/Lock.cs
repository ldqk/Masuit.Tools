using StackExchange.Redis;
using System;

namespace Masuit.Tools.Systems
{
    /// <summary>
    /// 分布式锁
    /// </summary>
    public class Lock
    {
        /// <summary>
        /// 分布式锁
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="val"></param>
        /// <param name="validity"></param>
        public Lock(RedisKey resource, RedisValue val, TimeSpan validity)
        {
            Resource = resource;
            Value = val;
            Validity = validity;
        }

        /// <summary>
        /// 
        /// </summary>
        public RedisKey Resource { get; }

        /// <summary>
        /// 
        /// </summary>
        public RedisValue Value { get; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Validity { get; }
    }
}