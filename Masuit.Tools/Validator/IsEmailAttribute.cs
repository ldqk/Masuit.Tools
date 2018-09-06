using System.ComponentModel.DataAnnotations;

namespace Masuit.Tools.Validator
{
    public class IsEmailAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                ErrorMessage = "邮箱不能为空！";
                return false;
            }

            var email = value as string;
            if (email.Length < 6)
            {
                ErrorMessage = "您输入的邮箱格式不正确！";
                return false;
            }

            if (email.Length > 256)
            {
                ErrorMessage = "邮箱长度最大允许255个字符！";
                return false;
            }
            if (email.MatchEmail())
            {
                return true;
            }
            ErrorMessage = "您输入的邮箱格式不正确！";
            return false;
        }
    }
}
