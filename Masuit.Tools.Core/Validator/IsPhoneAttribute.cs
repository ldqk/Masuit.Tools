using System.ComponentModel.DataAnnotations;

namespace Masuit.Tools.Core.Validator
{
    /// <summary>
    /// 验证手机号码是否合法
    /// </summary>
    public class IsPhoneAttribute : ValidationAttribute
    {
        /// <summary>
        /// 验证手机号码是否合法
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool IsValid(object value)
        {
            if (value is null)
            {
                ErrorMessage = "手机号码不能为空";
                return false;
            }
            string phone = value as string;
            if (phone.MatchPhoneNumber())
            {
                return true;
            }
            ErrorMessage = "手机号码格式不正确，请输入有效的大陆11位手机号码！";
            return false;
        }
    }
}