using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Masuit.Tools.Systems
{
    /// <summary>
    /// Redis分布式锁
    /// </summary>
    public class RedisLock : IDisposable
    {
        private bool _isDisposed;

        /// <summary>
        /// 终结器
        /// </summary>
        ~RedisLock()
        {
            Dispose(false);
        }

        /// <summary>
        /// KEYS[1] ：需要加锁的key，这里需要是字符串类型。
        /// ARGV[1] ：锁的超时时间，防止死锁
        /// ARGV[2] ：锁的唯一标识
        /// </summary>
        private const String LockScript = @"
                if (redis.call('exists', KEYS[1]) == 0) then 
                    redis.call('hset', KEYS[1], ARGV[2], 1);
                    redis.call('pexpire', KEYS[1], ARGV[1]);
                    return nil;
                end;
                if (redis.call('hexists', KEYS[1], ARGV[2]) == 1) then 
                    redis.call('hincrby', KEYS[1], ARGV[2], 1);
                    redis.call('pexpire', KEYS[1], ARGV[1]);
                    return nil;
                end;
                return redis.call('pttl', KEYS[1]);
            ";

        /// <summary>
        /// – KEYS[1] ：需要加锁的key，这里需要是字符串类型。
        /// – KEYS[2] ：redis消息的ChannelName,一个分布式锁对应唯一的一个channelName:“redisson_lock__channel__{” + getName() + “}”
        /// – ARGV[1] ：reids消息体，这里只需要一个字节的标记就可以，主要标记redis的key已经解锁，再结合redis的Subscribe，能唤醒其他订阅解锁消息的客户端线程申请锁。
        /// – ARGV[2] ：锁的超时时间，防止死锁
        /// – ARGV[3] ：锁的唯一标识
        /// </summary>
        private const String UnLockScript = @"
                if (redis.call('exists', KEYS[1]) == 0) then 
                    redis.call('publish', KEYS[2], ARGV[1]); 
                    return 1;
                end;
                if (redis.call('hexists', KEYS[1], ARGV[3]) == 0) then 
                    return nil;
                end;
                local counter = redis.call('hincrby', KEYS[1], ARGV[3], -1);
                if (counter > 0) then
                    redis.call('pexpire', KEYS[1], ARGV[2]); 
                    return 0;
                else
                    redis.call('del', KEYS[1]);
                    redis.call('publish', KEYS[2], ARGV[1]);
                    return 1;
                end;
                return nil;
            ";

        private const double ClockDriveFactor = 0.01;

        /// <summary>
        /// 默认的30秒过期时间
        /// </summary>
        private readonly TimeSpan _leaseTimeSpan = new TimeSpan(0, 0, 30);

        private readonly ConcurrentDictionary<string, CancellationTokenSource> _expirationRenewalMap = new ConcurrentDictionary<string, CancellationTokenSource>();

        private readonly ConnectionMultiplexer _server;

        /// <summary>
        /// 默认连接127.0.0.1:6379,synctimeout=20000
        /// </summary>
        /// <param name="connstr"></param>
        public RedisLock(string connstr = "127.0.0.1:6379,synctimeout=20000")
        {
            _server = ConnectionMultiplexer.Connect(connstr);
            _server.PreserveAsyncOrder = false;
        }


        /// <summary>
        /// 加锁
        /// </summary>
        /// <param name="resource">锁名</param>
        /// <param name="waitTimeSpan">如果没有锁成功,允许动重试申请锁的最大时长</param>
        /// <param name="leaseTimeSpan">如果锁成功,对于锁(key)的过期时间</param>
        /// <param name="lockObject">锁成功信息包装成对象返回</param>
        /// <returns>true:成功</returns>
        public bool TryLock(RedisKey resource, TimeSpan waitTimeSpan, TimeSpan leaseTimeSpan, out Lock lockObject)
        {
            lockObject = null;
            try
            {
                var startTime = DateTime.Now;
                var val = CreateUniqueLockId();
                //申请锁，返回还剩余的锁过期时间
                var ttl = TryAcquire(resource, val, leaseTimeSpan);
                var drift = Convert.ToInt32((waitTimeSpan.TotalMilliseconds * ClockDriveFactor) + 2);
                var validityTime = waitTimeSpan - (DateTime.Now - startTime) - new TimeSpan(0, 0, 0, 0, drift);

                // 如果为空，表示申请锁成功
                if (ttl.IsNull)
                {
                    lockObject = new Lock(resource, val, validityTime);
                    //开始一个调度程序
                    ScheduleExpirationRenewal(leaseTimeSpan, lockObject);
                    return true;
                }

                // 订阅监听redis消息
                Subscriber(resource);

                startTime = DateTime.Now;
                while (true)
                {
                    // 再次尝试一次申请锁
                    ttl = TryAcquire(resource, val, leaseTimeSpan);
                    // 获得锁，返回
                    if (ttl.IsNull)
                    {
                        lockObject = new Lock(resource, val, validityTime);
                        ScheduleExpirationRenewal(leaseTimeSpan, lockObject);
                        return true;
                    }

                    drift = Convert.ToInt32((waitTimeSpan.TotalMilliseconds * ClockDriveFactor) + 2);
                    validityTime = waitTimeSpan - (DateTime.Now - startTime) - new TimeSpan(0, 0, 0, 0, drift);
                    if (validityTime.TotalMilliseconds < 0)
                    {
                        //说明已经超过了客户端设置的最大wait time，则直接返回false，取消订阅，不再继续申请锁了。
                        //Console.WriteLine("已经超过了客户端设置的最大wait time,Thread ID:" + Thread.CurrentThread.ManagedThreadId);
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                // 无论是否获得锁,都要取消订阅解锁消息
                UnSubscriber(resource);
            }
        }


        /// <summary>
        /// 解锁
        /// </summary>
        /// <param name="lockObject">锁成功的返回对象</param>
        /// <returns></returns>
        public RedisResult UnLock(Lock lockObject)
        {
            if (lockObject == null)
            {
                return null;
            }

            CancelExpirationRenewal(lockObject);
            RedisKey[] key =
            {
                lockObject.Resource,
                GetChannelName(lockObject.Resource)
            };
            RedisValue[] values =
            {
                Thread.CurrentThread.ManagedThreadId,
                10000,
                lockObject.Value
            };
            return _server.GetDatabase().ScriptEvaluate(UnLockScript, key, values);
        }


        private void Subscriber(RedisKey resource)
        {
            ISubscriber sub = _server.GetSubscriber();
            sub.Subscribe(GetChannelName(resource), (channel, message) =>
            {
                //Console.WriteLine("Thread ID:" + aa + ",收到广播:Thread ID:" + message + " 已解锁");
            });
        }

        private void UnSubscriber(RedisKey resource)
        {
            ISubscriber sub = _server.GetSubscriber();
            sub.Unsubscribe(GetChannelName(resource));
        }

        private string GetChannelName(RedisKey resource)
        {
            return "redisson_lock__channel__{" + resource.ToString() + "}";
        }

        private RedisResult TryAcquire(RedisKey resource, string value, TimeSpan? leaseTimeSpan)
        {
            if (leaseTimeSpan != null)
            {
                return LockInnerAsync(resource, leaseTimeSpan.Value, value);
            }

            return LockInnerAsync(resource, value);
        }

        private RedisResult LockInnerAsync(RedisKey resource, TimeSpan waitTime, string threadId)
        {
            RedisKey[] key =
            {
                resource
            };
            RedisValue[] values =
            {
                waitTime.TotalMilliseconds,
                threadId
            };
            return _server.GetDatabase().ScriptEvaluate(LockScript, key, values);
        }

        private RedisResult LockInnerAsync(RedisKey resource, string threadId)
        {
            var task = LockInnerAsync(resource, _leaseTimeSpan, threadId);
            return task;
        }

        /// <summary>
        /// 创建唯一锁id
        /// </summary>
        /// <returns></returns>
        protected static string CreateUniqueLockId()
        {
            return string.Concat(Guid.NewGuid().ToString(), Thread.CurrentThread.ManagedThreadId);
        }

        /// <summary>
        /// 设置超时
        /// </summary>
        /// <param name="doWork"></param>
        /// <param name="time"></param>
        protected void SetTimeOut(ElapsedEventHandler doWork, int time)
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = time;
            timer.Elapsed += (sender, args) => timer.Stop();
            timer.Elapsed += doWork;
            timer.Start();
        }

        /// <summary>
        /// 任务超时
        /// </summary>
        /// <param name="action"></param>
        /// <param name="lockObj"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        protected CancellationTokenSource TaskTimeOut(Func<Lock, bool> action, Lock lockObj, int time)
        {
            var timeoutCancellationTokenSource = new CancellationTokenSource();
            Task.Run(() =>
            {
                SpinWait.SpinUntil(() => !timeoutCancellationTokenSource.IsCancellationRequested);
            }, timeoutCancellationTokenSource.Token);
            return timeoutCancellationTokenSource;
        }

        private void ScheduleExpirationRenewal(TimeSpan leaseTimeSpan, Lock lockObject)
        {
            ScheduleExpirationRenewal((lockObj) => _server.GetDatabase().KeyExpire(lockObj.Resource, leaseTimeSpan), lockObject, Convert.ToInt32(leaseTimeSpan.TotalMilliseconds) / 3);
        }

        private void ScheduleExpirationRenewal(Func<Lock, bool> action, Lock lockObj, int time)
        {
            // 保证任务不会被重复创建
            if (_expirationRenewalMap.ContainsKey(lockObj.Resource))
            {
                return;
            }

            var task = TaskTimeOut(action, lockObj, time);

            //如果已经存在，停止任务，也是为了在极端的并发情况下，保证任务不会被重复创建
            if (!_expirationRenewalMap.TryAdd(lockObj.Resource, task))
            {
                task.Cancel();
            }
        }

        private void CancelExpirationRenewal(Lock lockObj)
        {
            CancellationTokenSource task;
            if (_expirationRenewalMap.TryRemove(lockObj.Resource, out task))
            {
                task?.Cancel();
            }
        }

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="disposing"></param>
        public virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _server?.Dispose();
            _isDisposed = true;
        }
    }
}