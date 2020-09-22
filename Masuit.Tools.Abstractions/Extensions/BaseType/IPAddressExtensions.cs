using System.Net;
using System.Net.Sockets;

namespace Masuit.Tools
{
    public static class IPAddressExtensions
    {
        /// <summary>
        /// 判断IP是否是私有地址
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsPrivateIP(this IPAddress ip)
        {
            if (IPAddress.IsLoopback(ip)) return true;
            byte[] bytes = ip.GetAddressBytes();
            return ip.AddressFamily switch
            {
                AddressFamily.InterNetwork when bytes[0] == 10 => true,
                AddressFamily.InterNetwork when bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127 => true,
                AddressFamily.InterNetwork when bytes[0] == 169 && bytes[1] == 254 => true,
                AddressFamily.InterNetwork when bytes[0] == 172 && bytes[1] == 16 => true,
                AddressFamily.InterNetwork when bytes[0] == 192 && bytes[1] == 88 && bytes[2] == 99 => true,
                AddressFamily.InterNetwork when bytes[0] == 192 && bytes[1] == 168 => true,
                AddressFamily.InterNetwork when bytes[0] == 198 && bytes[1] == 18 => true,
                AddressFamily.InterNetwork when bytes[0] == 198 && bytes[1] == 51 && bytes[2] == 100 => true,
                AddressFamily.InterNetwork when bytes[0] == 203 && bytes[1] == 0 && bytes[2] == 113 => true,
                AddressFamily.InterNetworkV6 when ip.IsIPv6Teredo || ip.IsIPv6LinkLocal || ip.IsIPv6Multicast || ip.IsIPv6SiteLocal => true,
                AddressFamily.InterNetworkV6 when ip.ToString().StartsWith("::") => true,
                AddressFamily.InterNetworkV6 when ip.ToString().StartsWith("64:ff9b::") => true,
                AddressFamily.InterNetworkV6 when ip.ToString().StartsWith("100::") => true,
                AddressFamily.InterNetworkV6 when ip.ToString().StartsWith("2001::") => true,
                AddressFamily.InterNetworkV6 when ip.ToString().StartsWith("2001:2") => true,
                AddressFamily.InterNetworkV6 when ip.ToString().StartsWith("2001:db8:") => true,
                AddressFamily.InterNetworkV6 when ip.ToString().StartsWith("2002:") => true,
                AddressFamily.InterNetworkV6 when ip.ToString().StartsWith("fc") => true,
                AddressFamily.InterNetworkV6 when ip.ToString().StartsWith("fd") => true,
                AddressFamily.InterNetworkV6 when ip.ToString().StartsWith("fe") => true,
                AddressFamily.InterNetworkV6 when bytes[0] == 255 => true,
                _ => false
            };
        }
    }
}