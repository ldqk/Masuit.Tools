using System;
using System.Runtime.Serialization;

namespace Masuit.Tools.Models;

/// <summary>
/// 版本号
/// </summary>
[DataContract]
public sealed record VersionNumber : IComparable, IComparable<VersionNumber>
{
	/// <summary>
	/// 版本号
	/// </summary>
	/// <param name="major">主版本号</param>
	/// <param name="minor">子版本号</param>
	/// <param name="build">编译版本号</param>
	/// <param name="revision">修订版本号</param>
	public VersionNumber(uint major, uint minor, uint build, uint revision) : this(major, minor, build)
	{
		Revision = revision;
	}

	/// <summary>
	/// 版本号
	/// </summary>
	/// <param name="major">主版本号</param>
	/// <param name="minor">子版本号</param>
	/// <param name="build">编译版本号</param>
	public VersionNumber(uint major, uint minor, uint build) : this(major, minor)
	{
		Build = build;
	}

	/// <summary>
	/// 版本号
	/// </summary>
	/// <param name="major">主版本号</param>
	/// <param name="minor">子版本号</param>
	public VersionNumber(uint major, uint minor)
	{
		Major = major;
		Minor = minor;
	}

	/// <summary>
	/// 版本号
	/// </summary>
	public VersionNumber(string version)
	{
		if (Version.TryParse(version, out var v))
		{
			Major = (uint)Math.Max(0, v.Major);
			Minor = (uint)Math.Max(0, v.Minor);
			Build = (uint)Math.Max(0, v.Build);
			Revision = (uint)Math.Max(0, v.Revision);
		}
	}

	/// <summary>
	/// 版本号
	/// </summary>
	public VersionNumber(double version)
	{
		Major = (uint)version;
		Minor = (uint)((version - Major) * 10);
	}

	/// <summary>
	/// 字符串转换成版本号
	/// </summary>
	/// <param name="v"></param>
	/// <returns></returns>
	public static VersionNumber Parse(string v)
	{
		return new VersionNumber(v);
	}

	/// <summary>
	/// 字符串尝试转换成版本号
	/// </summary>
	/// <param name="v"></param>
	/// <param name="version"></param>
	/// <returns></returns>
	public static bool TryParse(string v, out VersionNumber version)
	{
		if (Version.TryParse(v, out var ver))
		{
			version = new VersionNumber((uint)Math.Max(0, ver.Major), (uint)Math.Max(0, ver.Minor), (uint)Math.Max(0, ver.Build), (uint)Math.Max(0, ver.Revision));
			return true;
		}

		version = null;
		return false;
	}

	/// <summary>
	/// 主版本号
	/// </summary>
	public uint Major { get; }

	/// <summary>
	/// 子版本号
	/// </summary>
	public uint Minor { get; }

	/// <summary>
	/// 编译版本号
	/// </summary>
	public uint Build { get; }

	/// <summary>
	/// 修订版本号
	/// </summary>
	public uint Revision { get; }

	public short MajorRevision => (short)(Revision >> 16);

	public short MinorRevision => (short)(Revision & 0xFFFF);

	public int CompareTo(object version)
	{
		if (version == null)
		{
			return 1;
		}

		if (version is VersionNumber v)
		{
			return CompareTo(v);
		}

		throw new ArgumentException(nameof(version));
	}

	public int CompareTo(VersionNumber value)
	{
		return ReferenceEquals(value, this) ? 0 : value is null ? 1 : Major != value.Major ? (Major > value.Major ? 1 : -1) : Minor != value.Minor ? (Minor > value.Minor ? 1 : -1) : Build != value.Build ? (Build > value.Build ? 1 : -1) : Revision != value.Revision ? (Revision > value.Revision ? 1 : -1) : 0;
	}

	public bool Equals(VersionNumber obj)
	{
		return ReferenceEquals(obj, this) || (!(obj is null) && Major == obj.Major && Minor == obj.Minor && Build == obj.Build && Revision == obj.Revision);
	}

	public override int GetHashCode()
	{
		uint accumulator = 0;
		accumulator |= (Major & 0x0000000F) << 28;
		accumulator |= (Minor & 0x000000FF) << 20;
		accumulator |= (Build & 0x000000FF) << 12;
		accumulator |= (Revision & 0x00000FFF);

		return (int)accumulator;
	}

	public override string ToString() => Major + "." + Minor + ("." + Build + "." + Revision).TrimEnd('.', '0');

	public static bool operator <(VersionNumber v1, VersionNumber v2)
	{
		if (v1 is null)
		{
			return v2 is not null;
		}

		return v1.CompareTo(v2) < 0;
	}

	public static bool operator <=(VersionNumber v1, VersionNumber v2)
	{
		if (v1 is null)
		{
			return true;
		}

		return v1.CompareTo(v2) <= 0;
	}

	public static bool operator >(VersionNumber v1, VersionNumber v2) => v2 < v1;

	public static bool operator >=(VersionNumber v1, VersionNumber v2) => v2 <= v1;

	public static VersionNumber operator +(VersionNumber v1, decimal v2)
	{
		int major = (int)v2;
		int minor = (int)((v2 - major) * 10);
		return new VersionNumber((uint)(v1.Major + major), (uint)(v1.Minor + minor), v1.Build, v1.Revision);
	}

	public static VersionNumber operator +(VersionNumber v1, double v2)
	{
		int major = (int)v2;
		int minor = (int)((v2 - major) * 10);
		return new VersionNumber((uint)(v1.Major + major), (uint)(v1.Minor + minor), v1.Build, v1.Revision);
	}

	public static VersionNumber operator +(VersionNumber v1, int v2)
	{
		return new VersionNumber((uint)(v1.Major + v2), v1.Minor, v1.Build, v1.Revision);
	}

	public static VersionNumber operator +(VersionNumber v1, VersionNumber v2)
	{
		return new VersionNumber(v1.Major + v2.Major, v1.Minor + v2.Minor, v1.Build + v2.Build, v1.Revision + v2.Revision);
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="v"></param>
	/// <returns></returns>
	public static implicit operator VersionNumber(string v)
	{
		return new VersionNumber(v);
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="v"></param>
	/// <returns></returns>
	public static implicit operator string(VersionNumber v)
	{
		return v.ToString();
	}
}
