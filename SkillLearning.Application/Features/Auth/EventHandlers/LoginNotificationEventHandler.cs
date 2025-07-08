using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Domain.Events;

namespace SkillLearning.Application.Features.Auth.EventHandlers
{
    public class LoginNotificationEventHandler(IEmailSender emailSender)
    {
        public async Task HandleLoginEventAsync(UserLoginEvent loginEvent)
        {
            var subject = "Notificação de Login em sua Conta SkillLearning";
            var htmlBody = $@"
                <p>Olá, <strong>{loginEvent.Username}</strong>!</p>
                <p>Detectamos um novo login em sua conta SkillLearning.</p>
                <p><strong>Detalhes do Login:</strong></p>
                <ul>
                    <li><strong>Data/Hora:</strong> {loginEvent.Timestamp.ToLocalTime():dd/MM/yyyy HH:mm:ss}</li>
                    <li><strong>Endereço IP:</strong> {loginEvent.IpAddress}</li>
                    <li><strong>Dispositivo/Navegador:</strong> {loginEvent.UserAgent}</li>
                </ul>
                <p>Se você reconhece esta atividade, pode ignorar este e-mail.</p>
                <p>Se você NÃO reconhece esta atividade, por favor, entre em contato conosco imediatamente.</p>
                <p>Atenciosamente,</p>
                <p>A Equipe SkillLearning</p>
            ";

            await emailSender.SendEmailAsync(loginEvent.Email, subject, htmlBody);
        }
    }
}