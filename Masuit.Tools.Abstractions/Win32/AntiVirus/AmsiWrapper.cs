using System;
using System.Runtime.InteropServices;

namespace Masuit.Tools.Win32.AntiVirus;

internal static class AmsiWrapper
{
    [DllImport("Amsi.dll", EntryPoint = "AmsiInitialize", CallingConvention = CallingConvention.StdCall)]
    public static extern int AmsiInitialize([MarshalAs(UnmanagedType.LPWStr)] string appName, out nint amsiContext);

    [DllImport("Amsi.dll", EntryPoint = "AmsiUninitialize", CallingConvention = CallingConvention.StdCall)]
    public static extern void AmsiUninitialize(nint amsiContext);

    [DllImport("Amsi.dll", EntryPoint = "AmsiOpenSession", CallingConvention = CallingConvention.StdCall)]
    public static extern int AmsiOpenSession(nint amsiContext, out nint session);

    [DllImport("Amsi.dll", EntryPoint = "AmsiCloseSession", CallingConvention = CallingConvention.StdCall)]
    public static extern void AmsiCloseSession(nint amsiContext, nint session);

    [DllImport("Amsi.dll", EntryPoint = "AmsiScanBuffer", CallingConvention = CallingConvention.StdCall)]
    public static extern int AmsiScanBuffer(nint amsiContext, byte[] buffer, uint length, string contentName, nint session, out AmsiResult result);

    [DllImport("Amsi.dll", EntryPoint = "AmsiScanString", CallingConvention = CallingConvention.StdCall)]
    public static extern int AmsiScanString(nint amsiContext, [In()][MarshalAs(UnmanagedType.LPWStr)] string @string, [In()][MarshalAs(UnmanagedType.LPWStr)] string contentName, nint session, out AmsiResult result);
}
