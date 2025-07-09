using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using SkillLearning.Infrastructure.Configuration;
using SkillLearning.Infrastructure.Services;
using System.Text.Json.Nodes;

namespace SkillLearning.Tests.IntegrationTests
{
    public class EmailSenderTests : IAsyncLifetime
    {
        private readonly IContainer _mailHogContainer = new ContainerBuilder()
            .WithImage("mailhog/mailhog:latest")
            .WithPortBinding(8025, true)
            .WithPortBinding(1025, true)
            .Build();

        private EmailSender _emailSender = null!;
        private HttpClient _mailHogHttpClient = null!;

        public async Task InitializeAsync()
        {
            await _mailHogContainer.StartAsync();

            var emailSettings = new EmailSettings
            {
                SmtpServer = _mailHogContainer.Hostname,
                SmtpPort = _mailHogContainer.GetMappedPublicPort(1025),
                SenderName = "Test Sender",
                SenderEmail = "noreply@test.com"
            };

            var options = Options.Create(emailSettings);
            _emailSender = new EmailSender(options);

            var apiAddress = new System.Uri($"http://{_mailHogContainer.Hostname}:{_mailHogContainer.GetMappedPublicPort(8025)}");
            _mailHogHttpClient = new HttpClient { BaseAddress = apiAddress };
        }

        public async Task DisposeAsync()
            => await _mailHogContainer.StopAsync();

        [Fact]
        public async Task SendEmailAsync_ShouldDeliverEmail_WithCorrectContent()
        {
            // Arrange
            var to = "receiver@test.com";
            var subject = "Test Subject";
            var body = "<h1>Hello, World!</h1>";

            // Act
            await _emailSender.SendEmailAsync(to, subject, body);

            // Assert
            var response = await _mailHogHttpClient.GetStringAsync("/api/v2/messages");
            var messages = JsonNode.Parse(response);

            messages.Should().NotBeNull("a resposta da API do MailHog não deve ser nula.");
            messages["total"]?.GetValue<int>().Should().Be(1, "deve haver exatamente um e-mail na caixa de entrada.");
            messages["items"]?.AsArray().Should().HaveCount(1, "a lista de itens de e-mail deve conter um elemento.");

            var email = messages["items"]?[0];
            email.Should().NotBeNull();

            var toNode = email["To"]?[0];
            toNode.Should().NotBeNull();
            var toAddress = $"{toNode["Mailbox"]?.GetValue<string>()}@{toNode["Domain"]?.GetValue<string>()}";
            toAddress.Should().Be(to);

            var fromNode = email["From"];
            fromNode.Should().NotBeNull();
            var fromAddress = $"{fromNode["Mailbox"]?.GetValue<string>()}@{fromNode["Domain"]?.GetValue<string>()}";
            fromAddress.Should().Be("noreply@test.com");

            var contentNode = email["Content"];
            contentNode.Should().NotBeNull();
            contentNode["Headers"]?["Subject"]?[0]?.GetValue<string>().Should().Be(subject);
            contentNode["Body"]?.GetValue<string>().Should().Contain(body);
        }
    }
}