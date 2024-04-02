using System.ComponentModel.DataAnnotations;

namespace Masuit.Tools.Core.Validator;

/// <summary>
/// 验证企业的统一社会信用代码是否合法
/// </summary>
public class UnifiedSocialCreditCodeAttribute : ValidationAttribute
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
    public UnifiedSocialCreditCodeAttribute(string customMessage = null)
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
            ErrorMessage = _customMessage ?? "企业统一社会信用代码不能为空";
            return false;
        }

        string phone = value as string;
        if (phone.MatchUSCC())
        {
            return true;
        }

        ErrorMessage = _customMessage ?? "企业统一社会信用代码格式不正确，请输入有效的企业统一社会信用代码！";
        return false;
    }
}