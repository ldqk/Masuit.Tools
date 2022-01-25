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
            ip = ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip;
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
                AddressFamily.InterNetwork when bytes[0] >= 233 => true,
                AddressFamily.InterNetworkV6 when ip.IsIPv6Teredo || ip.IsIPv6LinkLocal || ip.IsIPv6Multicast || ip.IsIPv6SiteLocal || bytes[0] == 0 || bytes[0] >= 252 => true,
                _ => false
            };
        }
    }
}
