using System;
using System.Net.Mail;

namespace Masuit.Tools.Models
{
#pragma warning disable 1591
    public class Email
    {
        /// <summary>
        /// 发件人用户名
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// 发件人邮箱密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 发送服务器端口号，默认25
        /// </summary>
        public int SmtpPort { get; set; } = 25;
        /// <summary>
        /// 发送服务器地址
        /// </summary>
        public string SmtpServer { get; set; }
        /// <summary>
        /// 邮件标题
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// 邮件正文
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// 收件人，多个收件人用英文逗号隔开
        /// </summary>
        public string Tos { get; set; }
        /// <summary>
        /// 是否启用SSL
        /// </summary>
        public bool EnableSsl { get; set; }
        /// <summary>
        /// 邮件消息对象
        /// </summary>
        MailMessage GetClient
        {
            get
            {
                if (string.IsNullOrEmpty(Tos)) return null;
                MailMessage mailMessage = new MailMessage();
                //多个接收者                
                foreach (string _str in Tos.Split(','))
                {
                    mailMessage.To.Add(_str);
                }
                mailMessage.From = new MailAddress(Username, Username);
                mailMessage.Subject = Subject;
                mailMessage.Body = Body;
                mailMessage.IsBodyHtml = true;
                mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
                mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;
                mailMessage.Priority = MailPriority.High;
                return mailMessage;
            }
        }
        SmtpClient GetSmtpClient
        {
            get
            {
                return new SmtpClient
                {
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(Username, Password),
                    DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                    Host = SmtpServer,
                    Port = SmtpPort,
                    EnableSsl = EnableSsl,
                };
            }
        }
        //回调方法
        Action<string> actionSendCompletedCallback = null;

        /// <summary>
        /// 使用异步发送邮件
        /// </summary>
        /// <param name="completedCallback">邮件发送后的回调方法</param>
        /// <returns></returns>
        public void SendAsync(Action<string> completedCallback)
        {
            SmtpClient smtpClient = GetSmtpClient;
            MailMessage mailMessage = GetClient;
            if (smtpClient == null || mailMessage == null) return;
            Subject = Subject;
            Body = Body;
            EnableSsl = false;
            //发送邮件回调方法
            actionSendCompletedCallback = completedCallback;
            smtpClient.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
            try
            {
                smtpClient.SendAsync(mailMessage, "true");//异步发送邮件,如果回调方法中参数不为"true"则表示发送失败
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                smtpClient = null;
                mailMessage = null;
            }
        }
        /// <summary>
        /// 异步操作完成后执行回调方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendCompletedCallback(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //同一组件下不需要回调方法,直接在此写入日志即可
            //写入日志
            //return;
            if (actionSendCompletedCallback == null) return;
            string message = string.Empty;
            if (e.Cancelled)
            {
                message = "异步操作取消";
            }
            else if (e.Error != null)
            {
                message = (string.Format("UserState:{0},Message:{1}", (string)e.UserState, e.Error.ToString()));
            }
            else
                message = (string)e.UserState;
            //执行回调方法
            actionSendCompletedCallback(message);
        }
    }
#pragma warning restore 1591
}