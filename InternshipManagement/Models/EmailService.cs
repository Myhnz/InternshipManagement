using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Configuration;

namespace InternshipManagement.Models
{
    public class EmailService
    {
        private readonly string _emailAddress;
        private readonly string _password;

        public EmailService(string emailAddress, string password)
        {
            _emailAddress = emailAddress;
            _password = password;
        }

        public bool SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("james.hbsa@gmail.com");
                mailMessage.To.Add(toEmail);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;

                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
                smtpClient.Port = 587;
                smtpClient.Credentials = new System.Net.NetworkCredential("james.hbsa@gmail.com", "lxrt wner okzg zcbx");
                smtpClient.EnableSsl = true;
                smtpClient.Send(mailMessage);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}