using FluentResults;
using MediatR;
using SkillLearning.Application.Common.Models;

namespace SkillLearning.Application.Features.Auth.RefreshTokenUseCase
{
    public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<Result<AuthResultDto>>;
}