using FluentResults;
using MediatR;
using SkillLearning.Application.Common.Models;

namespace SkillLearning.Application.Features.Auth.Commands
{
    public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<Result<AuthResultDto>>;
}