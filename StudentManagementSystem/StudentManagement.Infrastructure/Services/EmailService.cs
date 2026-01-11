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
            var senderName = _config["SmtpSettings:SenderName"];
            var senderEmail = _config["SmtpSettings:SenderEmail"];

            if (string.IsNullOrWhiteSpace(senderEmail))
                throw new Exception("SmtpSettings:SenderEmail is null or empty. Kiểm tra lại appsettings.");

            if (string.IsNullOrWhiteSpace(to))
                throw new Exception("Tham số 'to' bị null hoặc rỗng.");

            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(senderName, senderEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject ?? string.Empty;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody ?? string.Empty
            };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _config["SmtpSettings:Server"],
                int.Parse(_config["SmtpSettings:Port"] ?? "587"),
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _config["SmtpSettings:Username"],
                _config["SmtpSettings:Password"]);

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
