using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Strings;
using System;

namespace Masuit.Tools.Systems
{
    /// <summary>
    /// 动态生产有规律的分布式ID
    /// </summary>
    public class SnowFlake
    {
        #region 私有字段

        private static long _machineId; //机器码
        private static long _dataId; //数据ID
        private static long _sequence; //计数从零开始
        private static long _lastTimestamp = -1L; //最后时间戳

        private const long Twepoch = 687888001020L; //唯一时间随机量

        private const long MachineIdBits = 5L; //机器码字节数
        private const long DataBits = 5L; //数据字节数
        private const long MaxMachineId = -1L ^ -1L << (int)MachineIdBits; //最大机器码
        private const long MaxDataBitId = -1L ^ (-1L << (int)DataBits); //最大数据字节数

        private const long SequenceBits = 12L; //计数器字节数，12个字节用来保存计数码        
        private const long MachineIdShift = SequenceBits; //机器码数据左移位数，就是后面计数器占用的位数
        private const long DatacenterIdShift = SequenceBits + MachineIdBits;
        private const long TimestampLeftShift = DatacenterIdShift + DataBits; //时间戳左移动位数就是机器码+计数器总字节数+数据字节数
        private const long SequenceMask = -1L ^ -1L << (int)SequenceBits; //一毫秒内可以产生计数，如果达到该值则等到下一毫秒在进行生成

        private static readonly object SyncRoot = new object(); //加锁对象
        private static NumberFormater _numberFormater = new NumberFormater(36);
        private static SnowFlake _snowFlake;

        #endregion

        /// <summary>
        /// 获取一个新的id
        /// </summary>
        public static string NewId => GetInstance().GetUniqueId();

        /// <summary>
        /// 创建一个实例
        /// </summary>
        /// <returns></returns>
        public static SnowFlake GetInstance()
        {
            return _snowFlake ??= new SnowFlake();
        }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public SnowFlake()
        {
            Snowflakes(0, -1);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="machineId">机器码</param>
        public SnowFlake(long machineId)
        {
            Snowflakes(machineId, -1);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="machineId">机器码</param>
        /// <param name="datacenterId">数据中心id</param>
        public SnowFlake(long machineId, long datacenterId)
        {
            Snowflakes(machineId, datacenterId);
        }

        private void Snowflakes(long machineId, long datacenterId)
        {
            if (machineId >= 0)
            {
                if (machineId > MaxMachineId)
                {
                    throw new Exception("机器码ID非法");
                }

                _machineId = machineId;
            }

            if (datacenterId >= 0)
            {
                if (datacenterId > MaxDataBitId)
                {
                    throw new Exception("数据中心ID非法");
                }

                _dataId = datacenterId;
            }
        }

        /// <summary>
        /// 设置数制格式化器
        /// </summary>
        /// <param name="nf"></param>
        public static void SetNumberFormater(NumberFormater nf)
        {
            _numberFormater = nf;
        }

        /// <summary>
        /// 获取长整形的ID
        /// </summary>
        /// <returns></returns>
        public long GetLongId()
        {
            lock (SyncRoot)
            {
                var timestamp = DateTime.UtcNow.GetTotalMilliseconds();
                if (_lastTimestamp == timestamp)
                {
                    //同一毫秒中生成ID
                    _sequence = (_sequence + 1) & SequenceMask; //用&运算计算该毫秒内产生的计数是否已经到达上限
                    if (_sequence == 0)
                    {
                        //一毫秒内产生的ID计数已达上限，等待下一毫秒
                        timestamp = DateTime.UtcNow.GetTotalMilliseconds();
                    }
                }
                else
                {
                    //不同毫秒生成ID
                    _sequence = 0L;
                }

                _lastTimestamp = timestamp; //把当前时间戳保存为最后生成ID的时间戳
                long id = ((timestamp - Twepoch) << (int)TimestampLeftShift) | (_dataId << (int)DatacenterIdShift) | (_machineId << (int)MachineIdShift) | _sequence;
                return id;
            }
        }

        /// <summary>
        /// 获取一个字符串表示形式的id
        /// </summary>
        /// <returns></returns>
        public string GetUniqueId()
        {
            return _numberFormater.ToString(GetLongId());
        }

        /// <summary>
        /// 获取一个字符串表示形式的id
        /// </summary>
        /// <param name="maxLength">最大长度，至少6位</param>
        /// <returns></returns>
        public string GetUniqueShortId(int maxLength = 8)
        {
            if (maxLength < 6)
            {
                throw new ArgumentException("最大长度至少需要6位");
            }

            string id = GetUniqueId();
            int index = id.Length - maxLength;
            if (index < 0)
            {
                index = 0;
            }

            return id.Substring(index);
        }
    }
}