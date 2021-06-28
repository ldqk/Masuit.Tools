using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Masuit.Tools.Strings
{
    /// <summary>
    /// 数制格式化器
    /// </summary>
    public class NumberFormater
    {
        /// <summary>
        /// 数制表示字符集
        /// </summary>
        private string Characters { get; set; }

        /// <summary>
        /// 进制长度
        /// </summary>
        public int Length => Characters.Length;

        /// <summary>
        /// 起始值偏移
        /// </summary>
        private readonly int _offset;

        /// <summary>
        /// 数制格式化器
        /// </summary>
        public NumberFormater()
        {
            Characters = "0123456789";
        }

        /// <summary>
        /// 数制格式化器
        /// </summary>
        /// <param name="characters">符号集</param>
        /// <param name="offset">起始值偏移</param>
        public NumberFormater(string characters, int offset = 0)
        {
            if (string.IsNullOrEmpty(characters))
            {
                throw new ArgumentException("符号集不能为空");
            }

            Characters = characters;
            _offset = offset;
        }

        /// <summary>
        /// 数制格式化器
        /// </summary>
        /// <param name="bin">进制</param>
        /// <param name="offset">起始值偏移</param>
        public NumberFormater(int bin, int offset = 0)
        {
            if (bin < 2)
            {
                bin = 2;
            }

            if (bin > 64)
            {
                throw new ArgumentException("默认进制最大支持64进制");
            }

            Characters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ+/".Substring(0, bin);
            _offset = offset;
        }

        /// <summary>
        /// 数字转换为指定的进制形式字符串
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public string ToString(long number)
        {
            List<string> result = new List<string>();
            long t = Math.Abs(number);
            while (t != 0)
            {
                var mod = t % Length;
                t = Math.Abs(t / Length);
                var character = Characters[Convert.ToInt32(mod) - _offset].ToString();
                result.Insert(0, character);
            }

            if (number < 0)
            {
                result.Insert(0, "-");
            }

            return string.Join("", result);
        }

        /// <summary>
        /// 数字转换为指定的进制形式字符串
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public string ToString(BigInteger number)
        {
            List<string> result = new List<string>();
            if (number < 0)
            {
                number = -number;
                result.Add("0");
            }

            BigInteger t = number;

            while (t != 0)
            {
                var mod = t % Length;
                t = BigInteger.Abs(BigInteger.Divide(t, Length));
                var character = Characters[(int)mod - _offset].ToString();
                result.Insert(0, character);
            }

            return string.Join("", result);
        }

        /// <summary>
        /// 指定字符串转换为指定进制的数字形式
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public long FromString(string str)
        {
            int j = 0;
            return new string(str.ToCharArray().Reverse().ToArray()).Where(ch => Characters.Contains(ch)).Sum(ch => (Characters.IndexOf(ch) + _offset) * (long)Math.Pow(Length, j++));
        }

        /// <summary>
        /// 指定字符串转换为指定进制的大数形式
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public BigInteger FromStringBig(string str)
        {
            int j = 0;
            var chars = new string(str.ToCharArray().Reverse().ToArray()).Where(ch => Characters.Contains(ch));
            return chars.Aggregate(BigInteger.Zero, (current, c) => current + (Characters.IndexOf(c) + _offset) * BigInteger.Pow(Length, j++));
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Length + "进制模式，进制符：" + Characters;
        }

        // 转换数字
        private static char ToNum(char x)
        {
            string strChnNames = "零一二三四五六七八九";
            string strNumNames = "0123456789";
            return strChnNames[strNumNames.IndexOf(x)];
        }

        // 转换万以下整数
        private static string ChangeInt(string x)
        {
            string[] strArrayLevelNames = { "", "十", "百", "千" };
            string ret = "";
            int i;
            for (i = x.Length - 1; i >= 0; i--)
            {
                if (x[i] == '0')
                {
                    ret = ToNum(x[i]) + ret;
                }
                else
                {
                    ret = ToNum(x[i]) + strArrayLevelNames[x.Length - 1 - i] + ret;
                }
            }

            while ((i = ret.IndexOf("零零", StringComparison.Ordinal)) != -1)
            {
                ret = ret.Remove(i, 1);
            }

            if (ret[ret.Length - 1] == '零' && ret.Length > 1)
            {
                ret = ret.Remove(ret.Length - 1, 1);
            }

            if (ret.Length >= 2 && ret.Substring(0, 2) == "一十")
            {
                ret = ret.Remove(0, 1);
            }

            return ret;
        }

        // 转换整数
        private static string ToInt(string x)
        {
            int len = x.Length;
            string result;
            string temp;
            if (len <= 4)
            {
                result = ChangeInt(x);
            }
            else if (len <= 8)
            {
                result = ChangeInt(x.Substring(0, len - 4)) + "万";
                temp = ChangeInt(x.Substring(len - 4, 4));
                if (temp.IndexOf("千", StringComparison.Ordinal) == -1 && !string.IsNullOrEmpty(temp))
                {
                    result += "零" + temp;
                }
                else
                {
                    result += temp;
                }
            }
            else
            {
                result = ChangeInt(x.Substring(0, len - 8)) + "亿";
                temp = ChangeInt(x.Substring(len - 8, 4));
                if (temp.IndexOf("千", StringComparison.Ordinal) == -1 && !string.IsNullOrEmpty(temp))
                {
                    result += "零" + temp;
                }
                else
                {
                    result += temp;
                }

                result += "万";
                temp = ChangeInt(x.Substring(len - 4, 4));
                if (temp.IndexOf("千", StringComparison.Ordinal) == -1 && !string.IsNullOrEmpty(temp))
                {
                    result += "零" + temp;
                }
                else
                {
                    result += temp;
                }
            }
            int i;
            if ((i = result.IndexOf("零万", StringComparison.Ordinal)) != -1)
            {
                result = result.Remove(i + 1, 1);
            }

            while ((i = result.IndexOf("零零", StringComparison.Ordinal)) != -1)
            {
                result = result.Remove(i, 1);
            }

            if (result[result.Length - 1] == '零' && result.Length > 1)
            {
                result = result.Remove(result.Length - 1, 1);
            }

            return result;
        }

        /// <summary>
        /// 转换为中文数字格式
        /// </summary>
        /// <param name="num">123.45</param>
        /// <returns></returns>
        public static string ToChineseNumber(double num)
        {
            var x = num.ToString(CultureInfo.CurrentCulture);
            if (x.Length == 0)
            {
                return "";
            }

            string result = "";
            if (x[0] == '-')
            {
                result = "负";
                x = x.Remove(0, 1);
            }
            if (x[0].ToString() == ".")
            {
                x = "0" + x;
            }

            if (x[x.Length - 1].ToString() == ".")
            {
                x = x.Remove(x.Length - 1, 1);
            }

            if (x.IndexOf(".") > -1)
            {
                result += ToInt(x.Substring(0, x.IndexOf("."))) + "点" + x.Substring(x.IndexOf(".") + 1).Aggregate("", (current, t) => current + ToNum(t));
            }
            else
            {
                result += ToInt(x);
            }

            return result;
        }

        /// <summary>
        /// 数字转中文金额大写
        /// </summary>
        /// <param name="number">22.22</param>
        /// <returns></returns>
        public static string ToChineseMoney(double number)
        {
            string s = number.ToString("#L#E#D#C#K#E#D#C#J#E#D#C#I#E#D#C#H#E#D#C#G#E#D#C#F#E#D#C#.0B0A");
            string d = Regex.Replace(s, @"((?<=-|^)[^1-9]*)|((?'z'0)[0A-E]*((?=[1-9])|(?'-z'(?=[F-L\.]|$))))|((?'b'[F-L])(?'z'0)[0A-L]*((?=[1-9])|(?'-z'(?=[\　　.]|$))))", "${b}${z}");
            return Regex.Replace(d, ".", m => "负元空零壹贰叁肆伍陆柒捌玖空空空空空空空分角拾佰仟萬億兆京垓秭穰"[m.Value[0] - '-'].ToString());
        }
    }
}