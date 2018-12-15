using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Masuit.Tools.Validator
{
    /// <summary>
    /// 强密码验证
    /// </summary>
    public class ComplexPassword : ValidationAttribute
    {
        /// <summary>
        /// 密码复杂度校验
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool IsValid(object value)
        {
            string pwd = value as string;
            if (pwd.Length <= 6)
            {
                ErrorMessage = "密码过短，至少需要6个字符！";
                return false;
            }
            var regex = new Regex(@"(?=.*[0-9])                     #必须包含数字
                                            (?=.*[a-zA-Z])                  #必须包含小写或大写字母
                                            (?=([\x21-\x7e]+)[^a-zA-Z0-9])  #必须包含特殊符号
                                            .{6,30}                         #至少6个字符，最多30个字符
                                            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
            if (regex.Match(pwd).Success)
            {
                return true;
            }
            ErrorMessage = "密码强度值不够，密码必须包含数字，必须包含小写或大写字母，必须包含至少一个特殊符号，至少6个字符，最多30个字符！";
            return false;
        }
    }
}