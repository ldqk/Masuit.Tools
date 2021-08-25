using DnsClient;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Masuit.Tools.Models
{
    public class EmailAddress
    {
        public string Username { get; set; }
        public string Domain { get; set; }

        public List<IPAddress> SmtpServers
        {
            get
            {
                var nslookup = new LookupClient();
                var query = nslookup.Query(Domain, QueryType.MX).Answers.MxRecords().SelectMany(r => Dns.GetHostAddresses(r.Exchange.Value)).ToList();
                return query.FindAll(ip => !ip.IsPrivateIP());
            }
        }

        public EmailAddress(string email)
        {
            var parts = email.Split('@');
            Username = parts[0];
            Domain = parts[1];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static implicit operator EmailAddress(string email)
        {
            return new EmailAddress(email);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        public static implicit operator string(EmailAddress email)
        {
            return email.ToString();
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Username + "@" + Domain;
        }
    }
}