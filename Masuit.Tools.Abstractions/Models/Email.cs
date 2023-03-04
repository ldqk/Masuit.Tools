using Masuit.Tools.Systems;
using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace Masuit.Tools.Models
{
#pragma warning disable 1591

	public class Email : Disposable
	{
		/// <summary>
		/// 发件人用户名
		/// </summary>
		public EmailAddress Username { get; set; }

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
		/// 是否启用SSL，默认已启用
		/// </summary>
		public bool EnableSsl { get; set; } = true;

		/// <summary>
		/// 附件
		/// </summary>
		public List<Attachment> Attachments { get; set; } = new List<Attachment>();

		private MailMessage MailMessage => GetClient();

		/// <summary>
		/// 邮件消息对象
		/// </summary>
		private MailMessage GetClient()
		{
			if (string.IsNullOrEmpty(Tos)) return null;
			var mailMessage = new MailMessage();

			//多个接收者
			foreach (var str in Tos.Split(','))
			{
				mailMessage.To.Add(str);
			}

			mailMessage.From = new MailAddress(Username, Username);
			mailMessage.Subject = Subject;
			mailMessage.Body = Body;
			mailMessage.IsBodyHtml = true;
			mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
			mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;
			mailMessage.Priority = MailPriority.High;
			foreach (var item in Attachments.AsNotNull())
			{
				mailMessage.Attachments.Add(item);
			}

			return mailMessage;
		}

		private SmtpClient SmtpClient => new SmtpClient
		{
			UseDefaultCredentials = false,
			EnableSsl = EnableSsl,
			Host = SmtpServer,
			Port = SmtpPort,
			Credentials = new System.Net.NetworkCredential(Username, Password),
			DeliveryMethod = SmtpDeliveryMethod.Network,
		};

		//回调方法
		private Action<string> _actionSendCompletedCallback;

		/// <summary>
		/// 使用异步发送邮件
		/// </summary>
		/// <param name="completedCallback">邮件发送后的回调方法</param>
		/// <returns></returns>
		public void SendAsync(Action<string> completedCallback)
		{
			if (MailMessage == null) return;

			//发送邮件回调方法
			_actionSendCompletedCallback = completedCallback;
			SmtpClient.SendCompleted += SendCompletedCallback;
			SmtpClient.SendAsync(MailMessage, "true"); //异步发送邮件,如果回调方法中参数不为"true"则表示发送失败
		}

		/// <summary>
		/// 使用同步发送邮件
		/// </summary>
		public void Send()
		{
			if (MailMessage == null) return;
			SmtpClient.Send(MailMessage); //异步发送邮件,如果回调方法中参数不为"true"则表示发送失败
			Dispose(true);
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
			if (_actionSendCompletedCallback == null) return;
			string message;
			if (e.Cancelled)
			{
				message = "异步操作取消";
			}
			else if (e.Error != null)
			{
				message = $"UserState:{(string)e.UserState},Message:{e.Error}";
			}
			else
			{
				message = (string)e.UserState;
			}

			//执行回调方法
			_actionSendCompletedCallback(message);
			Dispose(true);
		}

		/// <summary>
		/// 释放
		/// </summary>
		/// <param name="disposing"></param>
		public override void Dispose(bool disposing)
		{
			MailMessage?.Dispose();
			SmtpClient?.Dispose();
			Attachments.ForEach(a => a.Dispose());
		}
	}

#pragma warning restore 1591
}
