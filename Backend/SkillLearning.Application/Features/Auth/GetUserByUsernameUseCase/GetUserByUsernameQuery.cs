using FluentResults;
using MediatR;
using SkillLearning.Application.Common.Models;

namespace SkillLearning.Application.Features.Auth.GetUserByUsernameUseCase
{
    public record GetUserByUsernameQuery(string Username) : IRequest<Result<UserDto>>;
}