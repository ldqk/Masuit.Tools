using Masuit.Tools.Core.Config;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
        private string DomainWhiteList { get; }

        /// <summary>
        /// 可在appsetting.json中添加EmailDomainWhiteList配置邮箱域名白名单，逗号分隔
        /// </summary>
        /// <param name="valid">是否检查邮箱的有效性</param>
        public IsEmailAttribute(bool valid = true)
        {
            DomainWhiteList = CoreConfig.Configuration["EmailDomainWhiteList"] ?? "";
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

            var email = value as string;
            if (email.Length <= 10)
            {
                ErrorMessage = "您输入的邮箱格式不正确！";
                return false;
            }

            if (email.Length > 256)
            {
                ErrorMessage = "邮箱长度最大允许255个字符！";
                return false;
            }

            if (DomainWhiteList.Split(',', StringSplitOptions.RemoveEmptyEntries).Any(item => email.EndsWith("@" + item)))
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
