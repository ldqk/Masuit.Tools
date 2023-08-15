using System;

namespace Masuit.Tools.Win32.AntiVirus;

public class WindowsDefenderScanException : Exception
{
    public WindowsDefenderScanException() : base("Execute scan command exception")
    {
    }
}
