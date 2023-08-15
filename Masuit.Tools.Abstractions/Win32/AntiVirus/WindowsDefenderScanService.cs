using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Masuit.Tools.Files;
using Masuit.Tools.Systems;

namespace Masuit.Tools.Win32.AntiVirus;

public class WindowsDefenderScanService
{
	public WindowsDefenderScanService()
	{
		if (!Directory.Exists(SystemParameter.WindowsDefenderPath))
		{
			throw new PlatformNotSupportedException("Windows Defender not found");
		}

		if (!File.Exists($"{SystemParameter.WindowsDefenderPath}\\{SystemParameter.WindowsDefenderExeName}"))
		{
			throw new PlatformNotSupportedException("Windows Defender not found");
		}
	}

	/// <summary>
	/// 扫描文件流
	/// </summary>
	/// <param name="filePath"></param>
	public ScanResult ScanStream(Stream stream)
	{
		var temp = Path.Combine(Environment.GetEnvironmentVariable("temp"), SnowFlake.NewId);
		stream.SaveFile(temp);
		if (stream.CanSeek)
		{
			stream.Position = 0;
		}

		var result = ScanFile(temp);
		try
		{
			File.Delete(temp);
		}
		catch (Exception)
		{
			// ignored
		}

		return result;
	}

	/// <summary>
	/// 扫描文件
	/// </summary>
	/// <param name="filePath"></param>
	public ScanResult ScanFile(string filePath)
	{
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException();
		}
		try
		{
			//Scanning xxxxx  found 1 threats.
			//Scanning xxxxx  found no threats.
			var result = RunScanCommand(filePath);
			if (result.Contains("found no threats"))
			{
				return new ScanResult
				{
					Result = ResultCode.NotDetected,
				};
			}

			return new ScanResult
			{
				Result = ResultCode.Detected,
			};
		}
		catch (Exception ex)
		{
			return new ScanResult
			{
				Result = ResultCode.Exception,
				Msg = ex.Message
			};
		}
	}

	/// <summary>
	/// 扫描文件夹（不支持递文件夹嵌套文件夹的扫描）
	/// </summary>
	/// <param name="directoryPath"></param>
	/// <returns>如有威胁文件，只返回文件夹中有威胁的文件</returns>
	public ScanResult ScanDirectory(string directoryPath)
	{
		if (!Directory.Exists(directoryPath))
		{
			throw new DirectoryNotFoundException();
		}
		try
		{
			var files = Directory.GetFiles(directoryPath);

			//文件夹扫描
			var result = RunScanCommand(directoryPath);

			if (result.Contains("found no threats"))
			{
				return new ScanResult
				{
					Result = ResultCode.NotDetected,
				};
			}

			result = result.ToLower();
			var detectedFile = (from file in files
								let filePath = $"{file}\r\n"
								where result.Contains(filePath.ToLower())
								select file.Replace(directoryPath, "").Replace("\\", "")).ToList();

			//解析文件，找到有威胁的文件
			return new ScanResult
			{
				Result = ResultCode.Detected,
				Msg = string.Join(";", detectedFile)
			};
		}
		catch (Exception ex)
		{
			return new ScanResult
			{
				Result = ResultCode.Exception,
				Msg = ex.Message
			};
		}
	}

	/// <summary>
	/// 运行命令
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	private string RunScanCommand(string path)
	{
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			throw new PlatformNotSupportedException();
		}

		var proc = new Process();
		try
		{
			proc.StartInfo.FileName = "cmd.exe";
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.RedirectStandardInput = true;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.StartInfo.RedirectStandardError = true;
			proc.StartInfo.CreateNoWindow = true;
			proc.Start();

			var command = $"\"{SystemParameter.WindowsDefenderPath}\\{SystemParameter.WindowsDefenderExeName}\" -Scan -ScanType 3 -File \"{path}\" -DisableRemediation";
			proc.StandardInput.WriteLine(command);
			proc.StandardInput.WriteLine("exit");
			while (!proc.HasExited)
			{
				proc.WaitForExit(1000);
			}

			return proc.StandardOutput.ReadToEnd();
		}
		catch (Exception ex)
		{
			throw new WindowsDefenderScanException();
		}
		finally
		{
			proc.Close();
			proc.Dispose();
		}
	}
}
