using System.ComponentModel.DataAnnotations;

namespace Masuit.Tools.Core.Validator
{
    /// <summary>
    /// 验证手机号码是否合法
    /// </summary>
    public class IsPhoneAttribute : ValidationAttribute
    {
        /// <summary>
        /// 是否允许为空
        /// </summary>
        public bool AllowEmpty { get; set; }

        private readonly string _customMessage;

        /// <summary>
        ///
        /// </summary>
        /// <param name="customMessage">自定义错误消息</param>
        public IsPhoneAttribute(string customMessage = null)
        {
            _customMessage = customMessage;
        }

        /// <summary>
        /// 验证手机号码是否合法
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool IsValid(object value)
        {
            if (AllowEmpty)
            {
                switch (value)
                {
                    case null:
                    case string s when string.IsNullOrEmpty(s):
                        return true;
                }
            }

            if (value is null)
            {
                ErrorMessage = _customMessage ?? "手机号码不能为空";
                return false;
            }

            string phone = value as string;
            if (phone.MatchPhoneNumber())
            {
                return true;
            }

            ErrorMessage = _customMessage ?? "手机号码格式不正确，请输入有效的大陆11位手机号码！";
            return false;
        }
    }
}
