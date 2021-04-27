using Masuit.Tools.Config;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace Masuit.Tools.Core.Validator
{
    /// <summary>
    /// 邮箱校验
    /// </summary>
    public class IsEmailAttribute : ValidationAttribute
    {
        private readonly bool _valid;

        /// <summary>
        /// 域白名单
        /// </summary>
        private string WhiteList { get; }

        /// <summary>
        /// 域黑名单
        /// </summary>
        private string BlockList { get; }

        /// <summary>
        /// 可在配置文件AppSetting节中添加EmailDomainWhiteList配置邮箱域名白名单，EmailDomainBlockList配置邮箱域名黑名单，逗号分隔，每个单独的元素支持正则表达式
        /// </summary>
        /// <param name="valid">是否检查邮箱的有效性</param>
        public IsEmailAttribute(bool valid = true)
        {
            WhiteList = Regex.Replace(ConfigHelper.GetConfigOrDefault("EmailDomainWhiteList"), @"(\w)\.([a-z]+),?", @"$1\.$2!").Trim('!');
            BlockList = Regex.Replace(ConfigHelper.GetConfigOrDefault("EmailDomainBlockList"), @"(\w)\.([a-z]+),?", @"$1\.$2!").Trim('!');
            _valid = valid;
        }

        /// <summary>
        /// 邮箱校验
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                ErrorMessage = "邮箱不能为空！";
                return false;
            }

            var email = (string)value;
            if (email.Length < 7)
            {
                ErrorMessage = "您输入的邮箱格式不正确！";
                return false;
            }

            if (email.Length > 256)
            {
                ErrorMessage = "您输入的邮箱无效，请使用真实有效的邮箱地址！";
                return false;
            }

            if (!string.IsNullOrEmpty(BlockList) && BlockList.Split('!').Any(item => Regex.IsMatch(email, item)))
            {
                ErrorMessage = "您输入的邮箱无效，请使用真实有效的邮箱地址！";
                return false;
            }

            if (!string.IsNullOrEmpty(WhiteList) && WhiteList.Split('!').Any(item => Regex.IsMatch(email, item)))
            {
                return true;
            }

            if (email.MatchEmail(_valid).isMatch)
            {
                return true;
            }

            ErrorMessage = "您输入的邮箱格式不正确！";
            return false;
        }
    }
}