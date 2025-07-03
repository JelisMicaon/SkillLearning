using FluentValidation.TestHelper;
using SkillLearning.Application.Features.Auth.Commands;
using SkillLearning.Application.Features.Auth.Commands.Validators;

namespace SkillLearning.Tests.UnitTests
{
    public class LoginUserCommandValidatorTests
    {
        private readonly LoginUserCommandValidator _validator = new();

        [Fact]
        public void Should_have_error_when_Password_is_empty()
        {
            var command = new LoginUserCommand { Username = "user", Password = "" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(c => c.Password);
        }

        [Fact]
        public void Should_have_error_when_Username_is_null()
        {
            var command = new LoginUserCommand { Username = null! };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(c => c.Username);
        }

        [Fact]
        public void Should_have_error_when_Username_is_empty()
        {
            var command = new LoginUserCommand { Username = "" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(c => c.Username);
        }

        [Fact]
        public void Should_not_have_error_when_command_is_valid()
        {
            var command = new LoginUserCommand { Username = "user", Password = "password" };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}