using FluentAssertions;
using Moq;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Features.Auth.EventHandlersUseCase;
using SkillLearning.Domain.Events;

namespace SkillLearning.Tests.UnitTests
{
    public class LoginNotificationEventHandlerTests
    {
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly LoginNotificationEventHandler _handler;

        public LoginNotificationEventHandlerTests()
        {
            _emailSenderMock = new Mock<IEmailSender>();
            _handler = new LoginNotificationEventHandler(_emailSenderMock.Object);
        }

        [Fact]
        public async Task HandleLoginEventAsync_ShouldCallEmailSender_WithCorrectDetails()
        {
            var loginEvent = new UserLoginEvent(
                Guid.NewGuid(),
                "testuser",
                "test@example.com",
                DateTime.UtcNow,
                "127.0.0.1",
                "Test Browser"
            );

            string capturedSubject = "";
            string capturedBody = "";

            _emailSenderMock
                .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((to, subject, body) =>
                {
                    capturedSubject = subject;
                    capturedBody = body;
                })
                .Returns(Task.CompletedTask);

            await _handler.HandleLoginEventAsync(loginEvent);

            _emailSenderMock.Verify(s => s.SendEmailAsync(loginEvent.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            capturedSubject.Should().Be("Notificação de Login em sua Conta SkillLearning");
            capturedBody.Should().Contain(loginEvent.Username);
            capturedBody.Should().Contain(loginEvent.IpAddress);
            capturedBody.Should().Contain(loginEvent.UserAgent);
        }
    }
}