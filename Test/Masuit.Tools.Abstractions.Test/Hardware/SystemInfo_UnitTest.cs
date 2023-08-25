using System;
using System.Threading.Tasks;
using Masuit.Tools.Hardware;
using Xunit;

namespace Masuit.Tools.Abstractions;

public class SystemInfo_UnitTest
{
    [Fact]
    public void SystemInfo_IsWinPlatform()
    {
        var res = SystemInfo.IsWinPlatform;
        if (Environment.OSVersion.Platform is PlatformID.MacOSX or PlatformID.Unix)
        {
            Assert.True(res == false);
        }
        else
        {
            Assert.True(res);
        }
    }
    
    [Fact]
    public void SystemInfo_ProcessorCount_MoreThanZero()
    {
        int res = SystemInfo.ProcessorCount;
        Assert.True(res > 0);
    }
    
    [Fact]
    public void SystemInfo_CpuLoad_IfNotWinPlatform()
    {
        float res = SystemInfo.CpuLoad;
        Assert.True(res == 0);
    }
    
    [Fact]
    public async Task SystemInfo_GetCpuUsageForProcess_MoreThanZero()
    {
        double res = await SystemInfo.GetCpuUsageForProcess();
        Assert.True(res > 0);
    }

    [Fact]
    public void SystemInfo_GetProcessorData_IfNotWinPlatform()
    {
        string res = SystemInfo.GetProcessorData();
        Assert.True(res == "0.000%");
    }
    
    [Fact]
    public void SystemInfo_GetCPUTemperature_IfNotWinPlatform()
    {
        float res = SystemInfo.GetCPUTemperature();
        Assert.True(res == 0);
    }

    [Fact]
    public void SystemInfo_GetCpuCount_MoreThanZero()
    {
        double res = SystemInfo.GetCpuCount();
        Assert.True(res > 0);
    }
    
    [Fact]
    public void SystemInfo_GetCpuInfo_IfNotWinPlatform()
    {
        var res = SystemInfo.GetCpuInfo();
        Assert.True(res.Count == 0);
    }
    
    [Fact]
    public void SystemInfo_MemoryAvailable_IfNotWinPlatform()
    {
        var res = SystemInfo.MemoryAvailable;
        Assert.True(res == 0);
    }
    
    [Fact]
    public void SystemInfo_PhysicalMemory_IfNotWinPlatform()
    {
        var res = SystemInfo.PhysicalMemory;
        Assert.True(res == 0);
    }
    
    [Fact]
    public void SystemInfo_GetMemoryVData_IfNotWinPlatform()
    {
        var res = SystemInfo.GetMemoryVData();
        Assert.True(res == "0.000% (0.000 B / 0.000 B) ");
    }
    
    [Fact]
    public void SystemInfo_GetUsageVirtualMemory_IfNotWinPlatform()
    {
        var res = SystemInfo.GetUsageVirtualMemory();
        Assert.True(res == 0);
    }
    
    [Fact]
    public void SystemInfo_GetUsedVirtualMemory_IfNotWinPlatform()
    {
        var res = SystemInfo.GetUsedVirtualMemory();
        Assert.True(res == 0);
    }
    
    [Fact]
    public void SystemInfo_GetTotalVirtualMemory_IfNotWinPlatform()
    {
        var res = SystemInfo.GetTotalVirtualMemory();
        Assert.True(res == 0);
    }
    
    [Fact]
    public void SystemInfo_GetMemoryPData_IfNotWinPlatform()
    {
        var res = SystemInfo.GetMemoryPData();
        Assert.True(res == "");
    }
    
    [Fact]
    public void SystemInfo_GetTotalPhysicalMemory_IfNotWinPlatform()
    {
        var res = SystemInfo.GetTotalPhysicalMemory();
        Assert.True(res == 0);
    }
    
    [Fact]
    public void SystemInfo_GetFreePhysicalMemory_IfNotWinPlatform()
    {
        var res = SystemInfo.GetFreePhysicalMemory();
        Assert.True(res == 0);
    }
    
    [Fact]
    public void SystemInfo_GetUsedPhysicalMemory_IfNotWinPlatform()
    {
        var res = SystemInfo.GetUsedPhysicalMemory();
        Assert.True(res == 0);
    }
    
    [Fact]
    public void SystemInfo_GetDiskData_Read_IfNotWinPlatform()
    {
        var res = SystemInfo.GetDiskData(DiskData.Read);
        Assert.True(res == 0);
    }
    
    [Fact]
    public void SystemInfo_GetDiskData_Write_IfNotWinPlatform()
    {
        var res = SystemInfo.GetDiskData(DiskData.Write);
        Assert.True(res == 0);
    }
    
    [Fact]
    public void SystemInfo_GetDiskData_ReadAndWrite_IfNotWinPlatform()
    {
        var res = SystemInfo.GetDiskData(DiskData.ReadAndWrite);
        Assert.True(res == 0);
    }
    
    [Fact]
    public void SystemInfo_GetDiskInfo_IfNotWinPlatform()
    {
        var res = SystemInfo.GetDiskInfo();
        Assert.True(res.Count == 0);
    }
    
    [Fact]
    public void SystemInfo_GetNetData_Sent_IfNotWinPlatform()
    {
        var res = SystemInfo.GetNetData(NetData.Sent);
        Assert.True(res == 0);
    }
    
    [Fact]
    public void SystemInfo_GetNetData_Received_IfNotWinPlatform()
    {
        var res = SystemInfo.GetNetData(NetData.Received);
        Assert.True(res == 0);
    }
    
    [Fact]
    public void SystemInfo_GetNetData_ReceivedAndSent_IfNotWinPlatform()
    {
        var res = SystemInfo.GetNetData(NetData.ReceivedAndSent);
        Assert.True(res == 0);
    }
    
    [Fact]
    public void SystemInfo_GetMacAddress_IfNotWinPlatform()
    {
        var res = SystemInfo.GetMacAddress();
        Assert.True(res.Count == 0);
    }
    
    [Fact]
    public void SystemInfo_GetLocalUsedIP_IfNotWinPlatform()
    {
        var res = SystemInfo.GetLocalUsedIP();
        Assert.True(res == null);
    }
    
    [Fact]
    public void SystemInfo_GetIPAddressWMI_IfNotWinPlatform()
    {
        var res = SystemInfo.GetIPAddressWMI();
        Assert.True(res == "");
    }
    
    [Fact]
    public void SystemInfo_GetLocalIPs()
    {
        var res = SystemInfo.GetLocalIPs();
        Assert.True(res.Count > 0);
    }
    
    [Fact]
    public void SystemInfo_GetNetworkCardAddress_IfNotWinPlatform()
    {
        var res = SystemInfo.GetNetworkCardAddress();
        Assert.True(res == "");
    }
    
    [Fact]
    public void SystemInfo_BootTime_IfNotWinPlatform()
    {
        var res = SystemInfo.BootTime();
        Assert.True(res == DateTime.MinValue);
    }
    
    [Fact]
    public void SystemInfo_FindAllApps_IfNotWinPlatform()
    {
        var res = SystemInfo.FindAllApps(0);
        Assert.True(res == null);
    }
    
    [Fact]
    public void SystemInfo_GetSystemType()
    {
        var res = SystemInfo.GetSystemType();
        Assert.True(!string.IsNullOrEmpty(res));
    }
}