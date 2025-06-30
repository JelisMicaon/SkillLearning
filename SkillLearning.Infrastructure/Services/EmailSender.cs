using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Infrastructure.Configuration;

namespace SkillLearning.Infrastructure.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(
                        _emailSettings.SmtpServer,
                        _emailSettings.SmtpPort,
                        SecureSocketOptions.StartTlsWhenAvailable,
                        CancellationToken.None);

                await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword, CancellationToken.None);
                await client.SendAsync(message, CancellationToken.None);
                await client.DisconnectAsync(true, CancellationToken.None);
            }
        }
    }
}