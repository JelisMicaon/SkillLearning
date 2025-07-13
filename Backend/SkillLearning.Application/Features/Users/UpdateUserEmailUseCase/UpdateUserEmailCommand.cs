using FluentResults;
using MediatR;

namespace SkillLearning.Application.Features.Users.UpdateUserEmailUseCase
{
    public record UpdateUserEmailCommand(Guid UserId, string NewEmail) : IRequest<Result>;
}