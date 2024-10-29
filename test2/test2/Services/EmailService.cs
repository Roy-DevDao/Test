using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using test2.Data;
using test2.Models;

namespace test2.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;  // Lấy cấu hình từ appsettings.json
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // Lấy thông tin SMTP từ cấu hình
            var smtpServer = _configuration["EmailSettings:Host"];
            var smtpPortString = _configuration["EmailSettings:Port"];
            var senderEmail = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]);

            if (string.IsNullOrEmpty(smtpPortString))
            {
                throw new ArgumentNullException("SmtpPort", "SmtpPort cannot be null or empty. Please check your configuration.");
            }

            // Chuyển đổi cổng SMTP thành kiểu int
            var smtpPort = int.Parse(smtpPortString);

            // Ghi log tất cả các giá trị để kiểm tra
            Console.WriteLine($"SMTP Server: {smtpServer}");
            Console.WriteLine($"SMTP Port: {smtpPortString}");
            Console.WriteLine($"Sender Email: {senderEmail}");
            Console.WriteLine($"Password: {(string.IsNullOrEmpty(password) ? "Empty or Null" : "Loaded")}");

            // Tạo email mới sử dụng MimeKit
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("DocCare", "minhquangdn27@gmail.com"));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            // Tạo nội dung email HTML
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            // Sử dụng SmtpClient từ MailKit để gửi email
            using var client = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                // Kết nối tới SMTP server với TLS
                await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                // Xác thực với thông tin email người gửi
                await client.AuthenticateAsync(senderEmail, password);
                // Gửi email
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khi gửi email
                Console.WriteLine($"An error occurred while sending the email: {ex.Message}");
                throw;
            }
            finally
            {
                // Ngắt kết nối khỏi SMTP server
                await client.DisconnectAsync(true);
            }
        }
    }
}