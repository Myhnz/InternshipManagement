using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using InternshipManagement.Models;

namespace InternshipManagement.Models
{
    public class OTPStorage
    {
        private static Dictionary<string, Tuple<string, DateTime>> otpDictionary = new Dictionary<string, Tuple<string, DateTime>>();
        private readonly EmailService emailService;
        public OTPStorage()
        {
            this.emailService = new EmailService("james.hbsa@gmail.com", "lxrt wner okzg zcbx");
        }
        public string GenerateOTP()
        {
            // Tạo mã OTP ngẫu nhiên, ví dụ: 6 ký tự số
            Random rand = new Random();
            int otpLength = 6;
            string otp = rand.Next((int)Math.Pow(10, otpLength - 1), (int)Math.Pow(10, otpLength)).ToString();
            return otp;
        }
        public void SendOTPByEmail(string email, string otp)
        {
            string subject = "Mã OTP của bạn";
            string body = "Mã OTP của bạn là: " + otp;

            emailService.SendEmail(email, subject, body);
        }
        public static void AddOTP(string email, string otp)
        {
            otpDictionary[email] = Tuple.Create(otp, DateTime.Now); // Lưu thời gian tạo
        }

        public static string GetOTP(string email)
        {
            if (otpDictionary.ContainsKey(email))
            {
                Tuple<string, DateTime> otpInfo = otpDictionary[email];
                DateTime creationTime = otpInfo.Item2;
                if ((DateTime.Now - creationTime).TotalMinutes <= 5) // Kiểm tra thời gian sử dụng (ví dụ: 5 phút)
                {
                    return otpInfo.Item1;
                }
                else
                {
                    otpDictionary.Remove(email); // Xóa mã OTP nếu đã hết hạn
                }
            }
            return null;
        }

        public static void RemoveOTP(string email)
        {
            if (otpDictionary.ContainsKey(email))
            {
                otpDictionary.Remove(email);
            }
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