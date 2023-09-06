using System;
using System.Linq;
using System.Net.NetworkInformation;
using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Strings;

namespace Masuit.Tools.Systems;

/// <summary>
/// 改良版雪花id
/// </summary>
public class SnowFlakeNew
{
	private static long _workerId; //机器ID
	private const long Twepoch = 1692079923000L; //唯一时间随机量
	private static long Offset = 7783685984256L; //起始偏移量
	private static long _sequence;
	private const int SequenceBits = 12; //计数器字节数，10个字节用来保存计数码
	private const long SequenceMask = -1L ^ -1L << SequenceBits; //一微秒内可以产生计数，如果达到该值则等到下一微妙在进行生成
	private static long _lastTimestamp = -1L;
	private static readonly object LockObj = new object();
	private static NumberFormater _numberFormater = new NumberFormater(36);
	private static SnowFlakeNew _snowFlake;

	/// <summary>
	/// 获取一个新的id
	/// </summary>
	public static string NewId => GetInstance().GetUniqueId();

	/// <summary>
	/// 获取一个新的id
	/// </summary>
	public static long LongId => GetInstance().GetLongId();

	/// <summary>
	/// 创建一个实例
	/// </summary>
	/// <returns></returns>
	public static SnowFlakeNew GetInstance()
	{
		return _snowFlake ??= new SnowFlakeNew();
	}

	/// <summary>
	/// 默认构造函数
	/// </summary>
	public SnowFlakeNew()
	{ }

	/// <summary>
	/// 默认构造函数
	/// </summary>
	static SnowFlakeNew()
	{
		var bytes = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault().GetPhysicalAddress().GetAddressBytes();
		_workerId = bytes[4] << 2 | bytes[5];
	}

	/// <summary>
	/// 构造函数
	/// </summary>
	/// <param name="machineId">机器码</param>
	public SnowFlakeNew(int machineId)
	{
		SetMachienId(machineId);
	}

	public static void SetMachienId(long machineId)
	{
		if (machineId >= 0)
		{
			if (machineId > 1024)
			{
				throw new Exception("机器码ID非法");
			}

			_workerId = machineId;
		}
	}

	/// <summary>
	/// 设置起始偏移量
	/// </summary>
	/// <param name="offset"></param>
	public static void SetInitialOffset(long offset)
	{
		Offset = offset;
	}

	/// <summary>
	/// 设置数制格式化器
	/// </summary>
	/// <param name="nf"></param>
	public static void SetNumberFormater(NumberFormater nf)
	{
		_numberFormater = nf;
	}

	public long GetLongId()
	{
		lock (LockObj)
		{
			long timestamp = DateTime.Now.GetTotalMilliseconds();
			if (_lastTimestamp == timestamp)
			{ //同一微妙中生成ID
				_sequence = (_sequence + 1) & SequenceMask; //用&运算计算该微秒内产生的计数是否已经到达上限
				if (_sequence == 0)
				{
					//一微妙内产生的ID计数已达上限，等待下一微妙
					timestamp = DateTime.Now.GetTotalMilliseconds();
					while (timestamp <= _lastTimestamp)
					{
						timestamp = DateTime.Now.GetTotalMilliseconds();
					}
					return timestamp;
				}
			}
			else
			{ //不同微秒生成ID
				_sequence = 0; //计数清0
			}
			if (timestamp < _lastTimestamp)
			{ //如果当前时间戳比上一次生成ID时时间戳还小，抛出异常，因为不能保证现在生成的ID之前没有生成过
				throw new Exception($"Clock moved backwards.  Refusing to generate id for {_lastTimestamp - timestamp} milliseconds");
			}
			_lastTimestamp = timestamp; //把当前时间戳保存为最后生成ID的时间戳
			return (_workerId << 52 | (timestamp - Twepoch << 12) | _sequence) - Offset;
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
