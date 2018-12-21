using System;
using System.Collections.Generic;
using System.Linq;

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
        public int Length => Characters?.Length ?? 0;

        public NumberFormater()
        {
            Characters = "0123456789";
        }

        /// <summary>
        /// 数制格式化器
        /// </summary>
        /// <param name="characters">进制转换</param>
        public NumberFormater(string characters)
        {
            Characters = characters;
        }

        /// <summary>
        /// 数制格式化器
        /// </summary>
        /// <param name="bin">进制</param>
        public NumberFormater(int bin)
        {
            if (bin < 2)
            {
                bin = 2;
            }

            if (bin > 62)
            {
                throw new ArgumentException("进制最大支持62进制");
            }
            Characters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".Substring(0, bin);
        }

        /// <summary>
        /// 数字转换为指定的进制形式字符串
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public string ToString(long number)
        {
            List<string> result = new List<string>();
            long t = number;

            while (t > 0)
            {
                var mod = t % Length;
                t = Math.Abs(t / Length);
                var character = Characters[Convert.ToInt32(mod)].ToString();
                result.Insert(0, character);
            }

            return string.Join("", result.ToArray());
        }

        /// <summary>
        /// 指定字符串转换为指定进制的数字形式
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public long FromString(string str)
        {
            long result = 0;
            int j = 0;
            foreach (var ch in new string(str.ToCharArray().Reverse().ToArray()))
            {
                if (Characters.Contains(ch))
                {
                    result += Characters.IndexOf(ch) * (long)Math.Pow(Length, j);
                    j++;
                }
            }

            return result;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Length + "模式";
        }
    }
}