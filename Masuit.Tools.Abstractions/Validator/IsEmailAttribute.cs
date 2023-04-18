using DnsClient;
using Masuit.Tools.Config;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Masuit.Tools.Core.Validator
{
    /// <summary>
    /// 邮箱校验
    /// </summary>
    public class IsEmailAttribute : ValidationAttribute
    {
        private readonly bool _valid;
        private readonly string _customMessage;

        /// <summary>
        /// 域白名单
        /// </summary>
        private string WhiteList { get; }

        /// <summary>
        /// 域黑名单
        /// </summary>
        private string BlockList { get; }

        /// <summary>
        /// 是否允许为空
        /// </summary>
        public bool AllowEmpty { get; set; }

        /// <summary>
        /// 可在配置文件AppSetting节中添加EmailDomainWhiteList配置邮箱域名白名单，EmailDomainBlockList配置邮箱域名黑名单，英文分号(;)或感叹号(!)或逗号(,)分隔，每个单独的元素支持正则表达式
        /// </summary>
        /// <param name="valid">是否检查邮箱的有效性</param>
        /// <param name="customMessage">自定义错误消息</param>
        public IsEmailAttribute(bool valid = true, string customMessage = null)
        {
            WhiteList = Regex.Replace(ConfigHelper.GetConfigOrDefault("EmailDomainWhiteList"), @"(\w)\.([a-z]+),?", @"$1\.$2!").Trim('!');
            BlockList = Regex.Replace(ConfigHelper.GetConfigOrDefault("EmailDomainBlockList"), @"(\w)\.([a-z]+),?", @"$1\.$2!").Trim('!');
            _valid = valid;
            _customMessage = customMessage;
        }

        /// <summary>
        /// 邮箱校验
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

            if (value == null)
            {
                ErrorMessage = _customMessage ?? "邮箱不能为空！";
                return false;
            }

            var email = (string)value;
            if (email.Length < 7)
            {
                ErrorMessage = _customMessage ?? "您输入的邮箱格式不正确！";
                return false;
            }

            if (email.Length > 256)
            {
                ErrorMessage = _customMessage ?? "您输入的邮箱无效，请使用真实有效的邮箱地址！";
                return false;
            }

            if (!string.IsNullOrEmpty(BlockList) && BlockList.Split(new[] { '!', ';' }, StringSplitOptions.RemoveEmptyEntries).Any(item => Regex.IsMatch(email, item)))
            {
                ErrorMessage = _customMessage ?? "您输入的邮箱无效，请使用真实有效的邮箱地址！";
                return false;
            }

            if (!string.IsNullOrEmpty(WhiteList) && WhiteList.Split('!').Any(item => Regex.IsMatch(email, item)))
            {
                return true;
            }

            var isMatch = email.MatchEmail().isMatch;
            if (isMatch && _valid)
            {
                var nslookup = new LookupClient();
                var records = nslookup.Query(email.Split('@')[1], QueryType.MX).Answers.MxRecords().ToList();
                if (!string.IsNullOrEmpty(BlockList) && records.Any(r => BlockList.Split('!').Any(item => Regex.IsMatch(r.Exchange.Value, item))))
                {
                    ErrorMessage = _customMessage ?? "您输入的邮箱无效，请使用真实有效的邮箱地址！";
                    return false;
                }

                var task = records.SelectAsync(r => Dns.GetHostAddressesAsync(r.Exchange.Value).ContinueWith(t =>
                {
                    if (t.IsCanceled || t.IsFaulted)
                    {
                        return new[] { IPAddress.Loopback };
                    }

                    return t.Result;
                }));
                isMatch = task.Result.SelectMany(a => a).Any(ip => !ip.IsPrivateIP());
            }
            if (isMatch)
            {
                return true;
            }

            ErrorMessage = _customMessage ?? "您输入的邮箱格式不正确！";
            return false;
        }
    }
}
