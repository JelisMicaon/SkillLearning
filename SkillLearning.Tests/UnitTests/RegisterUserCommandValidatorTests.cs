using FluentValidation.TestHelper;
using SkillLearning.Application.Features.Auth.Commands;
using SkillLearning.Application.Features.Auth.Commands.Validators;

namespace SkillLearning.Tests.UnitTests
{
    public class RegisterUserCommandValidatorTests
    {
        private readonly RegisterUserCommandValidator _validator;

        public RegisterUserCommandValidatorTests()
        {
            _validator = new RegisterUserCommandValidator();
        }

        [Fact]
        public void Should_have_error_when_Email_is_invalid()
        {
            var command = new RegisterUserCommand { Email = "email-invalido" };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(c => c.Email)
                  .WithErrorMessage("Formato de email inválido.");
        }

        [Fact]
        public void Should_have_error_when_Password_is_too_short()
        {
            var command = new RegisterUserCommand { Password = "123" };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(c => c.Password)
                  .WithErrorMessage("Password deve ter pelo menos 6 caracteres.");
        }

        [Fact]
        public void Should_have_error_when_Username_is_empty()
        {
            var command = new RegisterUserCommand { Username = "" };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(c => c.Username)
                  .WithErrorMessage("Username é obrigatório.");
        }

        [Theory]
        [InlineData("a")]
        [InlineData("ab")]
        public void Should_have_error_when_Username_is_too_short(string invalidUsername)
        {
            // Arrange
            var command = new RegisterUserCommand { Username = invalidUsername };

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(c => c.Username)
                  .WithErrorMessage("Username deve ter pelo menos 3 caracteres.");
        }

        [Fact]
        public void Should_not_have_error_when_command_is_valid()
        {
            var command = new RegisterUserCommand
            {
                Username = "usuariovalido",
                Email = "email@valido.com",
                Password = "senhasegura123"
            };

            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}