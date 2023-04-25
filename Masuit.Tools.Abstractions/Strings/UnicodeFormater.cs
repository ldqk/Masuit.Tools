using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace Masuit.Tools.Strings
{
    /// <summary>
    /// 数制格式化器
    /// </summary>
    public class UnicodeFormater
    {
        /// <summary>
        /// 数制表示字符集
        /// </summary>
        private List<string> Characters { get; }

        /// <summary>
        /// 进制长度
        /// </summary>
        public int Length => Characters.Count;

        /// <summary>
        /// 起始值偏移
        /// </summary>
        private readonly byte _offset;

        private readonly bool _emojiMode;

        private static readonly Regex EmojiRegex = new Regex(@"(\u00a9|\u00ae|[\u2000-\u3300]|\ud83c[\ud000-\udfff]|\ud83d[\ud000-\udfff]|\ud83e[\ud000-\udfff])");

        /// <summary>
        /// 数制格式化器
        /// </summary>
        public UnicodeFormater()
        {
            Characters = new List<string>() { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        }

        /// <summary>
        /// 数制格式化器
        /// </summary>
        /// <param name="characters">符号集</param>
        /// <param name="offset">起始值偏移</param>
        public UnicodeFormater(string characters, byte offset = 0)
        {
            if (string.IsNullOrEmpty(characters))
            {
                throw new ArgumentException("符号集不能为空");
            }

            var matches = EmojiRegex.Matches(characters);
            if (matches.Count > 0)
            {
                _emojiMode = true;
                Characters = new List<string>();
                foreach (Match m in matches)
                {
                    Characters.Add(m.Value);
                }
            }
            else
            {
                Characters = characters.Select(c => c.ToString()).ToList();
            }

            _offset = offset;
        }

        /// <summary>
        /// 数制格式化器
        /// </summary>
        /// <param name="characters">符号集</param>
        /// <param name="offset">起始值偏移</param>
        public UnicodeFormater(List<string> characters, byte offset = 0)
        {
            if (characters.IsNullOrEmpty())
            {
                throw new ArgumentException("符号集不能为空");
            }

            Characters = characters;
            _offset = offset;
        }

#if NET5_0_OR_GREATER

        /// <summary>
        /// 数制格式化器
        /// </summary>
        /// <param name="characters">符号集</param>
        /// <param name="offset">起始值偏移</param>
        public UnicodeFormater(ReadOnlySpan<byte> characters, byte offset = 0)
        {
            if (characters == null || characters.Length == 0)
            {
                throw new ArgumentException("符号集不能为空");
            }

            Characters = Encoding.UTF8.GetString(characters).Select(c => c.ToString()).ToList();
            _offset = offset;
        }

        /// <summary>
        /// 数制格式化器
        /// </summary>
        /// <param name="characters">符号集</param>
        /// <param name="offset">起始值偏移</param>
        public UnicodeFormater(ReadOnlySpan<char> characters, byte offset = 0)
        {
            if (characters == null || characters.Length == 0)
            {
                throw new ArgumentException("符号集不能为空");
            }

            Characters = new string(characters).Select(c => c.ToString()).ToList();;
            _offset = offset;
        }

#endif

        /// <summary>
        /// 数制格式化器
        /// </summary>
        /// <param name="characters">符号集</param>
        /// <param name="offset">起始值偏移</param>
        public UnicodeFormater(byte[] characters, byte offset = 0)
        {
            if (characters == null || characters.Length == 0)
            {
                throw new ArgumentException("符号集不能为空");
            }

            Characters = Encoding.UTF8.GetString(characters).Select(c => c.ToString()).ToList(); ;
            _offset = offset;
        }

        /// <summary>
        /// 数制格式化器
        /// </summary>
        /// <param name="characters">符号集</param>
        /// <param name="offset">起始值偏移</param>
        public UnicodeFormater(char[] characters, byte offset = 0)
        {
            if (characters == null || characters.Length == 0)
            {
                throw new ArgumentException("符号集不能为空");
            }

            Characters = characters.Select(c => c.ToString()).ToList(); ;
            _offset = offset;
        }

        /// <summary>
        /// 数制格式化器
        /// </summary>
        /// <param name="base">进制</param>
        /// <param name="offset">起始值偏移</param>
        public UnicodeFormater(byte @base, byte offset = 0)
        {
            Characters = @base switch
            {
                <= 2 => new List<string>() { "0", "1" },
                > 2 and < 65 => "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ+/".Substring(0, @base).Select(c => c.ToString()).ToList(),
                >= 65 and <= 91 => "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,-.:;<=>?@[]^_`{|}~\"".Substring(0, @base).Select(c => c.ToString()).ToList(),
                _ => throw new ArgumentException("默认进制最大支持91进制")
            };

            if (offset >= @base)
            {
                throw new ArgumentException("偏移量不能超过进制基数" + @base);
            }

            _offset = offset;
        }

        /// <summary>
        /// 数字转换为指定的进制形式字符串
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public string ToString(long number)
        {
            int start = 0;
            int resultOffset = 0;
            if (_offset > 0)
            {
                start = 1;
                resultOffset = _offset - 1;
            }

            number -= resultOffset;
            List<string> result = new List<string>();
            long t = Math.Abs(number);
            while (t != 0)
            {
                var mod = t % Length;
                t = Math.Abs(t / Length);
                var character = Characters[Convert.ToInt32(mod) - start].ToString();
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
            int start = 0;
            int resultOffset = 0;
            if (_offset > 0)
            {
                start = 1;
                resultOffset = _offset - 1;
            }

            number = number - resultOffset;
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
                var character = Characters[(int)mod - start].ToString();
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
            byte start = 0;
            int resultOffset = 0;
            if (_offset > 0)
            {
                start = 1;
                resultOffset = _offset - 1;
            }

            int j = 0;
            if (_emojiMode)
            {
                var emoji = new List<string>();
                foreach (Match m in EmojiRegex.Matches(str))
                {
                    emoji.Add(m.Value);
                }

                emoji.Reverse();
                return emoji.Where(Characters.Contains).Sum(ch => (Characters.IndexOf(ch) + start) * (long)Math.Pow(Length, j++)) + resultOffset;
            }

            var chars = str.ToCharArray();
            Array.Reverse(chars);
            return chars.Where(c => Characters.Contains(c.ToString())).Sum(ch => (Characters.IndexOf(ch.ToString()) + start) * (long)Math.Pow(Length, j++)) + resultOffset;
        }

        /// <summary>
        /// 指定字符串转换为指定进制的大数形式
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public BigInteger FromStringBig(string str)
        {
            byte start = 0;
            int resultOffset = 0;
            if (_offset > 0)
            {
                start = 1;
                resultOffset = _offset - 1;
            }
            int j = 0;
            if (_emojiMode)
            {
                var emoji = new List<string>();
                foreach (Match m in EmojiRegex.Matches(str))
                {
                    emoji.Add(m.Value);
                }
                emoji.Reverse();
                return emoji.Where(Characters.Contains).Aggregate(BigInteger.Zero, (current, c) => current + (Characters.IndexOf(c) + start) * BigInteger.Pow(Length, j++)) + resultOffset;
            }

            var charArray = str.ToCharArray();
            Array.Reverse(charArray);
            var chars = charArray.Where(c => Characters.Contains(c.ToString()));
            return chars.Aggregate(BigInteger.Zero, (current, c) => current + (Characters.IndexOf(c.ToString()) + start) * BigInteger.Pow(Length, j++)) + resultOffset;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Length + "进制模式，进制符：" + Characters.Join("");
        }
    }
}
