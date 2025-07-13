using FluentResults;
using MediatR;

namespace SkillLearning.Application.Features.Auth.CheckUserExistsUseCase
{
    public record CheckUserExistsQuery(string Username, string Email) : IRequest<Result<bool>>;
}