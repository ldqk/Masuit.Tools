//using System;
//using System.ComponentModel.DataAnnotations;
//using System.Linq;
//using Masuit.Tools.Core.Config;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;

//namespace Masuit.Tools.Core.Validator
//{
//    /// <summary>
//    /// 邮箱校验
//    /// </summary>
//    public class IsEmailAttribute : ValidationAttribute
//    {
//        private readonly bool _valid;
//        public static string EmailAllowKeywordsList;

//        /// <summary>
//        /// 域白名单
//        /// </summary>
//        private string DomainWhiteList { get; }

//        /// <summary>
//        /// 可在appsetting.json中添加EmailDomainWhiteList配置邮箱域名白名单，逗号分隔
//        /// </summary>
//        /// <param name="valid">是否检查邮箱的有效性</param>
//        public IsEmailAttribute(bool valid = true)
//        {
//            this.DomainWhiteList = CoreConfig.Configuration["EmailDomainWhiteList"] ?? "";
//            this._valid = valid;
//        }

//        /// <summary>
//        /// 邮箱校验
//        /// </summary>
//        /// <param name="value"></param>
//        /// <returns></returns>
//        public override bool IsValid(object value)
//        {
//            if (value == null)
//            {
//                this.ErrorMessage = "邮箱不能为空！";
//                return false;
//            }

//            string email = value as string;
//            if (email.Length <= 10)
//            {
//                this.ErrorMessage = "您输入的邮箱格式不正确！";
//                return false;
//            }

//            if (email.Length > 256)
//            {
//                this.ErrorMessage = "邮箱长度最大允许255个字符！";
//                return false;
//            }

//            if (this.DomainWhiteList.Split(',', StringSplitOptions.RemoveEmptyEntries).Any(item => email.EndsWith("@" + item)))
//            {
//                return true;
//            }

//            if (email.MatchEmail(this._valid).isMatch)
//            {
//                return true;
//            }

//            this.ErrorMessage = "您输入的邮箱格式不正确！";
//            return false;
//        }
//    }

//    public static class TestEE
//    {
//        public static IServiceCollection ConfigMasuitTools(IServiceCollection services, Action<MasuitToolOption> option)
//        {
//            MasuitToolOption opt = new MasuitToolOption();
//            option?.Invoke(opt);
//            IsEmailAttribute.EmailAllowKeywordsList = opt.EmailAllowKeywordsList;

//            return services;
//        }

//        public static IServiceCollection ConfigMasuitTools(IServiceCollection services, IConfiguration configuration)
//        {
//            IsEmailAttribute.EmailAllowKeywordsList = configuration["EmailAllowKeywordsList"] ?? "";

//            return services;
//        }
//    }

//    public class MasuitToolOption
//    {
//        /// <summary>
//        /// 邮件地址关键字白名单
//        /// </summary>
//        public string EmailAllowKeywordsList { get; set; }
//    }
//}