using System;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using Masuit.Utilities.Models;

namespace Masuit.Tools.Net
{
    /// <summary>
    /// Web操作扩展
    /// </summary>
    public static class WebExtension
    {
        #region 获取线程内唯一的EF上下文对象

        /// <summary>
        /// 获取线程内唯一的EF上下文对象
        /// </summary>
        /// <typeparam name="T">EF上下文容器对象</typeparam>
        /// <returns>EF上下文容器对象</returns>
        public static T GetDbContext<T>() where T : new()
        {
            T db;
            if (CallContext.GetData("db") == null) //由于CallContext比HttpContext先存在，所以首选CallContext为线程内唯一对象
            {
                db = new T();
                CallContext.SetData("db", db);
            }
            db = (T)CallContext.GetData("db");
            return db;
        }
        #endregion

        #region 发送邮件

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="mail">邮箱对象</param>
        /// <returns>发送成功</returns>
        public static bool Sendmail(this Email mail)
        {
            using (MailMessage msg = new MailMessage())
            {
                msg.To.Add(mail.To);
                msg.From = new MailAddress(mail.MailAccount, mail.Subject);
                msg.Subject = mail.Subject; //邮件标题
                msg.SubjectEncoding = Encoding.UTF8; //邮件标题编码
                msg.Body = mail.Body; //邮件内容
                msg.BodyEncoding = Encoding.UTF8; //邮件内容编码
                msg.IsBodyHtml = mail.IsHtml; //是否是HTML邮件
                msg.Priority = MailPriority.High; //邮件优先级
                SmtpClient client = new SmtpClient();
                client.Credentials = new NetworkCredential(mail.MailAccount, mail.Password);
                client.Host = mail.Smtp;
                object userState = msg;
                try
                {
                    client.Send(msg);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="mail">邮箱对象</param>
        /// <returns>发送成功</returns>
        public static bool SendmailAsync(this Email mail)
        {
            using (MailMessage msg = new MailMessage())
            {
                msg.To.Add(mail.To);
                msg.From = new MailAddress(mail.MailAccount, mail.Subject);
                msg.Subject = mail.Subject; //邮件标题
                msg.SubjectEncoding = Encoding.UTF8; //邮件标题编码
                msg.Body = mail.Body; //邮件内容
                msg.BodyEncoding = Encoding.UTF8; //邮件内容编码
                msg.IsBodyHtml = mail.IsHtml; //是否是HTML邮件
                msg.Priority = MailPriority.High; //邮件优先级
                SmtpClient client = new SmtpClient();
                client.Credentials = new NetworkCredential(mail.MailAccount, mail.Password);
                client.Host = mail.Smtp;
                object userState = msg;
                try
                {
                    client.SendAsync(msg, userState);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        #endregion

        #region 写Session

        /// <summary>
        /// 写Session
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void SetSession(string key, dynamic value) => HttpContext.Current.Session[key] = value;

        #endregion

        #region 获取Session

        /// <summary>
        /// 获取Session
        /// </summary>
        /// <typeparam name="T">对象</typeparam>
        /// <param name="key">键</param>
        /// <returns>对象</returns>
        public static T GetSession<T>(string key) => (T)HttpContext.Current.Session[key];

        #endregion

        #region 获取客户端IP地址信息

        /// <summary>
        /// 获取客户端IP地址信息
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>详细的地理位置、运营商信息</returns>
        public static async Task<IPData> GetIPAddressInfoAsync(this string ip)
        {
            HttpClient client = new HttpClient();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            INetAddress ipAddress; //IP地址对象变量声明
            Match match = Regex.Match(ip, @"\d+\.\d+\.\d+\.\d+"); //IP地址正则
            if (match.Success)
            {
                try
                {
                    string ipData = await client.GetStringAsync($"http://ip.taobao.com/service/getIpInfo.php?ip={ip}").ConfigureAwait(false); //根据API地址获取
                    ipAddress = serializer.Deserialize<INetAddress>(ipData); //将获取到的json数据反序列化为对象
                    return ipAddress.data;
                }
                catch (Exception)
                {
                    return new INetAddress { data = { region = "未能获取到IP地址信息" } }.data; //如果发生异常，则构造一个空对象
                }
            }

            return new INetAddress { data = { region = "IP地址格式不正确" } }.data; //如果发生异常，则构造一个空对象
        }

        #endregion
    }
}