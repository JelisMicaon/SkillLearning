using FluentResults;
using MediatR;

namespace SkillLearning.Application.Features.Auth.Commands
{
    public class RegisterUserCommand : IRequest<Result>
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}