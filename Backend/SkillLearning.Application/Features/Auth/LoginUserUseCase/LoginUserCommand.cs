using FluentResults;
using MediatR;
using SkillLearning.Application.Common.Models;

namespace SkillLearning.Application.Features.Auth.LoginUserUseCase
{
    public class LoginUserCommand : IRequest<Result<AuthResultDto>>
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}