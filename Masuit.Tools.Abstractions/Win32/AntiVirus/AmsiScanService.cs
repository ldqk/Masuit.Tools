using System;
using System.Buffers;
using System.IO;
using static Masuit.Tools.Win32.AntiVirus.AmsiWrapper;

namespace Masuit.Tools.Win32.AntiVirus;

public static class AmsiScanService
{
	private const string AppName = "AMSI";

	static AmsiScanService()
	{
		if (!File.Exists(SystemParameter.AmsiDllPath))
		{
			throw new PlatformNotSupportedException("amsi.dll not found");
		}
	}

	/// <summary>
	/// 扫描文件
	/// </summary>
	/// <param name="filePath"></param>
	/// <returns></returns>
	public static ScanResult Scan(string filePath)
	{
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException();
		}

		try
		{
			AmsiInitialize(AppName, out var amsiContext);
			AmsiOpenSession(amsiContext, out var amsiSession);

			//读取文件（如文件有可疑，会引发Exception）
			var fileData = File.ReadAllBytes(filePath);
			return ScanBuffer(amsiContext, amsiSession, fileData, Path.GetFileName(filePath));
		}
		catch (Exception ex)
		{
			if (ex.Message.Contains("Operation did not complete successfully because the file contains a virus or potentially unwanted software"))
			{
				return new ScanResult
				{
					Result = ResultCode.Detected
				};
			}

			return new ScanResult
			{
				Result = ResultCode.Exception,
				Msg = ex.Message
			};
		}
	}

	/// <summary>
	/// 扫描文件流
	/// </summary>
	/// <param name="fileStream"></param>
	/// <returns></returns>
	public static ScanResult Scan(Stream fileStream)
	{
		if (fileStream == null)
		{
			throw new ArgumentNullException(nameof(fileStream));
		}

		var fileData = ArrayPool<byte>.Shared.Rent((int)fileStream.Length);
		try
		{
			AmsiInitialize(AppName, out var amsiContext);
			AmsiOpenSession(amsiContext, out var amsiSession);
			_ = fileStream.Read(fileData, 0, (int)fileStream.Length);
			return ScanBuffer(amsiContext, amsiSession, fileData, "");
		}
		catch (Exception ex)
		{
			if (ex.Message.Contains("Operation did not complete successfully because the file contains a virus or potentially unwanted software"))
			{
				return new ScanResult
				{
					Result = ResultCode.Detected
				};
			}

			return new ScanResult
			{
				Result = ResultCode.Exception,
				Msg = ex.Message
			};
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(fileData);
		}
	}

	/// <summary>
	/// 扫描文件
	/// </summary>
	/// <param name="fileData"></param>
	/// <returns></returns>
	public static ScanResult Scan(byte[] fileData)
	{
		if (fileData == null)
		{
			throw new ArgumentNullException(nameof(fileData));
		}

		try
		{
			AmsiInitialize(AppName, out nint amsiContext);
			AmsiOpenSession(amsiContext, out nint amsiSession);
			return ScanBuffer(amsiContext, amsiSession, fileData, "");
		}
		catch (Exception ex)
		{
			if (ex.Message.Contains("Operation did not complete successfully because the file contains a virus or potentially unwanted software"))
			{
				return new ScanResult
				{
					Result = ResultCode.Detected
				};
			}

			return new ScanResult
			{
				Result = ResultCode.Exception,
				Msg = ex.Message
			};
		}
	}

	/// <summary>
	/// 扫描文件Buffer
	/// </summary>
	/// <param name="amsiContext"></param>
	/// <param name="amsiSession"></param>
	/// <param name="bufferData"></param>
	/// <param name="fileName"></param>
	/// <returns></returns>
	private static ScanResult ScanBuffer(nint amsiContext, nint amsiSession, byte[] bufferData, string fileName)
	{
		AmsiScanBuffer(amsiContext, bufferData, (uint)bufferData.Length, fileName, amsiSession, out AmsiResult amsiResult);
		var result = ConvertToScanResult(amsiResult);
		return result;
	}

	private static ScanResult ConvertToScanResult(AmsiResult amsiResult)
	{
		switch (amsiResult)
		{
			case AmsiResult.AmsiResultClean:
			case AmsiResult.AmsiResultNotDetected:
				return new ScanResult
				{
					Result = ResultCode.NotDetected
				};

			case AmsiResult.AmsiResultDetected:
				return new ScanResult
				{
					Result = ResultCode.Detected
				};

			default:
				return new ScanResult
				{
					Result = ResultCode.Exception,
					Msg = "Blocked behavior,scan not allowed!"
				};
		}
	}
}
