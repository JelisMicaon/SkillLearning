using FluentValidation;

namespace SkillLearning.Application.Features.Auth.Commands.Validators
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(command => command.Username)
                .NotEmpty().WithMessage("Username é obrigatório.")
                .MinimumLength(3).WithMessage("Username deve ter pelo menos 3 caracteres.")
                .MaximumLength(50).WithMessage("Username não pode exceder 50 caracteres.");

            RuleFor(command => command.Email)
                .NotEmpty().WithMessage("Email é obrigatório.")
                .EmailAddress().WithMessage("Formato de email inválido.")
                .MaximumLength(100).WithMessage("Email não pode exceder 100 caracteres.");

            RuleFor(command => command.Password)
                .NotEmpty().WithMessage("Password é obrigatório.")
                .MinimumLength(6).WithMessage("Password deve ter pelo menos 6 caracteres.")
                .MaximumLength(100).WithMessage("Password não pode exceder 100 caracteres.");
        }
    }
}