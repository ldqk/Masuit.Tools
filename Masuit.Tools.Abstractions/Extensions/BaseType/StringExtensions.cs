using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using DnsClient;
using Masuit.Tools.Strings;

namespace Masuit.Tools
{
    public static partial class StringExtensions
    {
        /// <summary>
        /// 字符串转时间
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string value)
        {
            DateTime.TryParse(value, out var result);
            return result;
        }

        /// <summary>
        /// 字符串转Guid
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Guid ToGuid(this string s)
        {
            return Guid.Parse(s);
        }

        /// <summary>
        /// 根据正则替换
        /// </summary>
        /// <param name="input"></param>
        /// <param name="regex">正则表达式</param>
        /// <param name="replacement">新内容</param>
        /// <returns></returns>
        public static string Replace(this string input, Regex regex, string replacement)
        {
            return regex.Replace(input, replacement);
        }

        /// <summary>
        /// 生成唯一短字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="chars">可用字符数数量，0-9,a-z,A-Z</param>
        /// <returns></returns>
        public static string CreateShortToken(this string str, byte chars = 36)
        {
            var nf = new NumberFormater(chars);
            return nf.ToString((DateTime.Now.Ticks - 630822816000000000) * 100 + Stopwatch.GetTimestamp() % 100);
        }

        /// <summary>
        /// 任意进制转十进制
        /// </summary>
        /// <param name="str"></param>
        /// <param name="bin">进制</param>
        /// <returns></returns>
        public static long ToBinary(this string str, int bin)
        {
            var nf = new NumberFormater(bin);
            return nf.FromString(str);
        }

        /// <summary>
        /// 任意进制转大数十进制
        /// </summary>
        /// <param name="str"></param>
        /// <param name="bin">进制</param>
        /// <returns></returns>
        public static BigInteger ToBinaryBig(this string str, int bin)
        {
            var nf = new NumberFormater(bin);
            return nf.FromStringBig(str);
        }

        #region 检测字符串中是否包含列表中的关键词

        /// <summary>
        /// 检测字符串中是否包含列表中的关键词
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="keys">关键词列表</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        public static bool Contains(this string s, IEnumerable<string> keys, bool ignoreCase = true)
        {
            if (!keys.Any() || string.IsNullOrEmpty(s))
            {
                return false;
            }

            if (ignoreCase)
            {
                return Regex.IsMatch(s, string.Join("|", keys), RegexOptions.IgnoreCase);
            }

            return Regex.IsMatch(s, string.Join("|", keys));
        }

        /// <summary>
        /// 判断是否包含符号
        /// </summary>
        /// <param name="str"></param>
        /// <param name="symbols"></param>
        /// <returns></returns>
        public static bool ContainsSymbol(this string str, params string[] symbols)
        {
            return str switch
            {
                null => false,
                string a when string.IsNullOrEmpty(a) => false,
                string a when a == string.Empty => false,
                _ => symbols.Any(t => str.Contains(t))
            };
        }

        #endregion 检测字符串中是否包含列表中的关键词

        /// <summary>
        /// 判断字符串是否为空或""
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        /// <summary>
        /// 字符串掩码
        /// </summary>
        /// <param name="s">字符串</param>
        /// <param name="mask">掩码符</param>
        /// <returns></returns>
        public static string Mask(this string s, char mask = '*')
        {
            if (string.IsNullOrWhiteSpace(s?.Trim()))
            {
                return s;
            }
            s = s.Trim();
            string masks = mask.ToString().PadLeft(4, mask);
            return s.Length switch
            {
                _ when s.Length >= 11 => Regex.Replace(s, @"(\w{3})\w*(\w{4})", $"$1{masks}$2"),
                _ when s.Length == 10 => Regex.Replace(s, @"(\w{3})\w*(\w{3})", $"$1{masks}$2"),
                _ when s.Length == 9 => Regex.Replace(s, @"(\w{2})\w*(\w{3})", $"$1{masks}$2"),
                _ when s.Length == 8 => Regex.Replace(s, @"(\w{2})\w*(\w{2})", $"$1{masks}$2"),
                _ when s.Length == 7 => Regex.Replace(s, @"(\w{1})\w*(\w{2})", $"$1{masks}$2"),
                _ when s.Length >= 2 && s.Length < 7 => Regex.Replace(s, @"(\w{1})\w*(\w{1})", $"$1{masks}$2"),
                _ => s + masks
            };
        }

        #region Email

        /// <summary>
        /// 匹配Email
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="valid">是否验证有效性</param>
        /// <returns>匹配对象；是否匹配成功，若返回true，则会得到一个Match对象，否则为null</returns>
        public static (bool isMatch, Match match) MatchEmail(this string s, bool valid = false)
        {
            if (string.IsNullOrEmpty(s) || s.Length < 7)
            {
                return (false, null);
            }

            Match match = Regex.Match(s, @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
            var isMatch = match.Success;
            if (isMatch && valid)
            {
                var nslookup = new LookupClient();
                var query = nslookup.Query(s.Split('@')[1], QueryType.MX).Answers.MxRecords().SelectMany(r => Dns.GetHostAddresses(r.Exchange.Value)).ToList();
                isMatch = query.Any(ip => !ip.IsPrivateIP());
            }

            return (isMatch, match);
        }

        /// <summary>
        /// 邮箱掩码
        /// </summary>
        /// <param name="s">邮箱</param>
        /// <param name="mask">掩码</param>
        /// <returns></returns>
        public static string MaskEmail(this string s, char mask = '*')
        {
            return !MatchEmail(s).isMatch ? s : s.Replace(s.Substring(0, s.LastIndexOf("@")), Mask(s.Substring(0, s.LastIndexOf("@")), mask));
        }

        #endregion Email

        #region 匹配完整的URL

        /// <summary>
        /// 匹配完整格式的URL
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="isMatch">是否匹配成功，若返回true，则会得到一个Match对象，否则为null</param>
        /// <returns>匹配对象</returns>
        public static Uri MatchUrl(this string s, out bool isMatch)
        {
            try
            {
                var uri = new Uri(s);
                isMatch = Dns.GetHostAddresses(uri.Host).Any(ip => !ip.IsPrivateIP());
                return uri;
            }
            catch
            {
                isMatch = false;
                return null;
            }
        }

        /// <summary>
        /// 匹配完整格式的URL
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>是否匹配成功</returns>
        public static bool MatchUrl(this string s)
        {
            MatchUrl(s, out var isMatch);
            return isMatch;
        }

        #endregion 匹配完整的URL

        #region 权威校验身份证号码

        /// <summary>
        /// 根据GB11643-1999标准权威校验中国身份证号码的合法性
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>是否匹配成功</returns>
        public static bool MatchIdentifyCard(this string s)
        {
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (s.Length == 18)
            {
                if (long.TryParse(s.Remove(17), out var n) == false || n < Math.Pow(10, 16) || long.TryParse(s.Replace('x', '0').Replace('X', '0'), out n) == false)
                {
                    return false; //数字验证
                }

                if (address.IndexOf(s.Remove(2), StringComparison.Ordinal) == -1)
                {
                    return false; //省份验证
                }

                string birth = s.Substring(6, 8).Insert(6, "-").Insert(4, "-");
                if (!DateTime.TryParse(birth, out _))
                {
                    return false; //生日验证
                }

                string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
                string[] wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
                char[] ai = s.Remove(17).ToCharArray();
                int sum = 0;
                for (int i = 0 ; i < 17 ; i++)
                {
                    sum += wi[i].ToInt32() * ai[i].ToString().ToInt32();
                }

                Math.DivRem(sum, 11, out var y);
                return arrVarifyCode[y] == s.Substring(17, 1).ToLower();
            }

            if (s.Length == 15)
            {
                if (long.TryParse(s, out var n) == false || n < Math.Pow(10, 14))
                {
                    return false; //数字验证
                }

                if (address.IndexOf(s.Remove(2), StringComparison.Ordinal) == -1)
                {
                    return false; //省份验证
                }

                string birth = s.Substring(6, 6).Insert(4, "-").Insert(2, "-");
                return DateTime.TryParse(birth, out _);
            }

            return false;
        }

        #endregion 权威校验身份证号码

        #region IP地址

        /// <summary>
        /// 校验IP地址的正确性，同时支持IPv4和IPv6
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="isMatch">是否匹配成功，若返回true，则会得到一个Match对象，否则为null</param>
        /// <returns>匹配对象</returns>
        public static IPAddress MatchInetAddress(this string s, out bool isMatch)
        {
            isMatch = IPAddress.TryParse(s, out var ip);
            return ip;
        }

        /// <summary>
        /// 校验IP地址的正确性，同时支持IPv4和IPv6
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>是否匹配成功</returns>
        public static bool MatchInetAddress(this string s)
        {
            MatchInetAddress(s, out var success);
            return success;
        }

        /// <summary>
        /// IP地址转换成数字
        /// </summary>
        /// <param name="addr">IP地址</param>
        /// <returns>数字,输入无效IP地址返回0</returns>
        public static uint IPToID(this string addr)
        {
            if (!IPAddress.TryParse(addr, out var ip))
            {
                return 0;
            }

            byte[] bInt = ip.GetAddressBytes();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bInt);
            }

            return BitConverter.ToUInt32(bInt, 0);
        }

        /// <summary>
        /// 判断IP是否是私有地址
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsPrivateIP(this string ip)
        {
            if (MatchInetAddress(ip))
            {
                return IPAddress.Parse(ip).IsPrivateIP();
            }
            throw new ArgumentException(ip + "不是一个合法的ip地址");
        }

        /// <summary>
        /// 判断IP地址在不在某个IP地址段
        /// </summary>
        /// <param name="input">需要判断的IP地址</param>
        /// <param name="begin">起始地址</param>
        /// <param name="ends">结束地址</param>
        /// <returns></returns>
        public static bool IpAddressInRange(this string input, string begin, string ends)
        {
            uint current = input.IPToID();
            return current >= begin.IPToID() && current <= ends.IPToID();
        }

        #endregion IP地址

        #region 校验手机号码的正确性

        /// <summary>
        /// 匹配手机号码
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="isMatch">是否匹配成功，若返回true，则会得到一个Match对象，否则为null</param>
        /// <returns>匹配对象</returns>
        public static Match MatchPhoneNumber(this string s, out bool isMatch)
        {
            if (string.IsNullOrEmpty(s))
            {
                isMatch = false;
                return null;
            }
            Match match = Regex.Match(s, @"^((1[3,5,6,8][0-9])|(14[5,7])|(17[0,1,3,6,7,8])|(19[8,9]))\d{8}$");
            isMatch = match.Success;
            return isMatch ? match : null;
        }

        /// <summary>
        /// 匹配手机号码
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>是否匹配成功</returns>
        public static bool MatchPhoneNumber(this string s)
        {
            MatchPhoneNumber(s, out bool success);
            return success;
        }

        #endregion 校验手机号码的正确性

        #region Url

        /// <summary>
        /// 判断url是否是外部地址
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsExternalAddress(this string url)
        {
            var uri = new Uri(url);
            switch (uri.HostNameType)
            {
                case UriHostNameType.Dns:
                    var ipHostEntry = Dns.GetHostEntry(uri.DnsSafeHost);
                    if (ipHostEntry.AddressList.Where(ipAddress => ipAddress.AddressFamily == AddressFamily.InterNetwork).Any(ipAddress => !ipAddress.IsPrivateIP()))
                    {
                        return true;
                    }
                    break;

                case UriHostNameType.IPv4:
                    return !IPAddress.Parse(uri.DnsSafeHost).IsPrivateIP();
            }
            return false;
        }

        #endregion Url

        /// <summary>
        /// 转换成字节数组
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(this string @this)
        {
            return Activator.CreateInstance<ASCIIEncoding>().GetBytes(@this);
        }

        #region Crc32

        /// <summary>
        /// 获取字符串crc32签名
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Crc32(this string s)
        {
            return string.Join(string.Empty, new Security.Crc32().ComputeHash(Encoding.UTF8.GetBytes(s)).Select(b => b.ToString("x2")));
        }

        /// <summary>
        /// 获取字符串crc64签名
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Crc64(this string s)
        {
            return string.Join(string.Empty, new Security.Crc64().ComputeHash(Encoding.UTF8.GetBytes(s)).Select(b => b.ToString("x2")));
        }

        #endregion Crc32
    }
}