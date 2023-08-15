using Masuit.Tools.Win32.AntiVirus;
using Microsoft.Extensions.DependencyInjection;

namespace Masuit.Tools.Windows.AntiVirus;

public static class RegisterFileSecurityScanSersvice
{
	/// <summary>
	///  注入Windows Defender服务
	/// </summary>
	/// <param name="services"></param>
	public static void AddWindowsDefender(this IServiceCollection services)
	{
		if (!Directory.Exists(SystemParameter.WindowsDefenderPath))
		{
			throw new PlatformNotSupportedException("Windows Defender not found");
		}

		if (!File.Exists($"{SystemParameter.WindowsDefenderPath}\\{SystemParameter.WindowsDefenderExeName}"))
		{
			throw new PlatformNotSupportedException("Windows Defender not found");
		}

		services.AddSingleton<WindowsDefenderScanService>();
	}

	/// <summary>
	/// 注入AMSI服务
	/// </summary>
	/// <param name="services"></param>
	public static void AddAMSI(this IServiceCollection services)
	{
		if (!File.Exists(SystemParameter.AmsiDllPath))
		{
			throw new PlatformNotSupportedException("amsi.dll not found");
		}

		services.AddSingleton<AmsiScanService>();
	}
}
