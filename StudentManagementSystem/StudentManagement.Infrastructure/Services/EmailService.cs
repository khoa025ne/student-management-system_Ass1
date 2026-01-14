using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace StudentManagement.Infrastructure.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            // Đọc cấu hình từ section "Email"
            var senderName = _config["Email:SenderName"];
            var senderEmail = _config["Email:SenderEmail"];
            var server = _config["Email:Server"];
            var portString = _config["Email:Port"];
            var username = _config["Email:Username"];
            var password = _config["Email:Password"];

            if (string.IsNullOrWhiteSpace(senderEmail))
                throw new Exception("Email:SenderEmail is null or empty. Kiểm tra lại appsettings.");

            if (string.IsNullOrWhiteSpace(server))
                throw new Exception("Email:Server is null or empty. Kiểm tra lại appsettings.");

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new Exception("Email:Username hoặc Email:Password bị thiếu. Kiểm tra lại appsettings.");

            if (string.IsNullOrWhiteSpace(to))
                throw new Exception("Tham số 'to' bị null hoặc rỗng.");

            if (!int.TryParse(portString, out var port))
                port = 587;

            // Tạo email
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(senderName ?? string.Empty, senderEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject ?? string.Empty;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody ?? string.Empty
            };
            email.Body = bodyBuilder.ToMessageBody();

            // Gửi mail bằng MailKit
            using var smtp = new SmtpClient();

            try
            {
                await smtp.ConnectAsync(
                    server,
                    port,
                    SecureSocketOptions.StartTls);

                await smtp.AuthenticateAsync(username, password);

                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                // Log chi tiết lỗi để debug nếu có vấn đề
                Console.WriteLine($"[EmailService] Error sending email: {ex}");
                throw; // cho bung lỗi ra ngoài trong giai đoạn dev để dễ phát hiện
            }
            finally
            {
                if (smtp.IsConnected)
                    await smtp.DisconnectAsync(true);
            }
        }
    }
}
