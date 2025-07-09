using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Infrastructure.Configuration;

namespace SkillLearning.Infrastructure.Services
{
    public class EmailSender(IOptions<EmailSettings> emailSettings) : IEmailSender
    {
        private readonly EmailSettings _settings = emailSettings.Value;

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var message = new MimeMessage
            {
                Subject = subject,
                Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody()
            };

            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));

            using var client = new SmtpClient();

            await client.ConnectAsync(
                _settings.SmtpServer,
                _settings.SmtpPort,
                SecureSocketOptions.StartTlsWhenAvailable);

            await client.AuthenticateAsync(_settings.SmtpUsername, _settings.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}