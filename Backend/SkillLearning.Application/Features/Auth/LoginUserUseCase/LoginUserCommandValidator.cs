using FluentValidation;

namespace SkillLearning.Application.Features.Auth.LoginUserUseCase
{
    public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
    {
        public LoginUserCommandValidator()
        {
            RuleFor(command => command.Username)
                .NotEmpty().WithMessage("Username é obrigatório.");

            RuleFor(command => command.Password)
                .NotEmpty().WithMessage("Password é obrigatório.");
        }
    }
}