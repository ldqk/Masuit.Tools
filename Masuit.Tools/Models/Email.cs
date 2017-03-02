namespace Masuit.Utilities.Models
{
#pragma warning disable 1591
    public class Email
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string MailAccount { get; set; }
        public string Password { get; set; }
        public string Smtp { get; set; }
        public string To { get; set; }
        public bool IsHtml { get; set; }
    }
#pragma warning restore 1591
}