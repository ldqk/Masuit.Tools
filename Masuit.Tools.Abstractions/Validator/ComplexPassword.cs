using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Masuit.Tools.Core.Validator
{
    /// <summary>
    /// 强密码验证
    /// </summary>
    public class ComplexPasswordAttribute : ValidationAttribute
    {
        /// <summary>
        /// 必须包含数字
        /// </summary>
        public bool MustNumber { get; set; } = true;

        /// <summary>
        /// 必须包含小写或大写字母
        /// </summary>
        public bool MustLetter { get; set; } = true;

        /// <summary>
        /// 必须包含特殊符号
        /// </summary>
        public bool MustSymbol { get; set; } = true;

        private readonly int _minLength = 6;
        private readonly int _maxLength = 30;

        public ComplexPasswordAttribute()
        {
        }

        public ComplexPasswordAttribute(int minLength, int maxLength)
        {
            _minLength = minLength;
            _maxLength = maxLength;
        }

        /// <summary>
        /// 校验密码强度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool IsValid(object value)
        {
            string pwd = value.ToString();
            if (pwd.Length < _minLength)
            {
                ErrorMessage = $"密码过短，至少需要{_minLength}个字符！";
                return false;
            }
            string regStr = "";
            if (MustNumber)
            {
                regStr += "(?=.*[0-9])"; //必须包含数字
            }

            if (MustLetter)
            {
                regStr += "(?=.*[a-zA-Z])";//必须包含小写或大写字母
            }

            if (MustSymbol)
            {
                regStr += @"(?=([\x21-\x7e]+)[^a-zA-Z0-9])";//必须包含特殊符号
            }

            var regex = new Regex($"{regStr}.{{{_minLength},{_maxLength}}}", RegexOptions.IgnorePatternWhitespace);
            if (regex.Match(pwd).Success)
            {
                return true;
            }

            string error = "密码强度值不够，密码";
            if (MustNumber && !Regex.IsMatch(pwd, "(?=.*[0-9])"))
            {
                error += "必须包含数字，";
            }

            if (MustLetter && !Regex.IsMatch(pwd, "(?=.*[a-zA-Z])"))
            {
                error += "必须包含小写或大写字母，";
            }

            if (MustSymbol && !Regex.IsMatch(pwd, @"(?=([\x21-\x7e]+)[^a-zA-Z0-9])"))
            {
                error += "必须包含至少一个特殊符号，";
            }

            ErrorMessage = error + $"至少{_minLength}个字符，最多{_maxLength}个字符！";
            return false;
        }
    }
}
