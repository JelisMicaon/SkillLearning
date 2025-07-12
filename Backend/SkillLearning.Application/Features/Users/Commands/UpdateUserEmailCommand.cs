using FluentResults;
using MediatR;

namespace SkillLearning.Application.Features.Users.Commands
{
    public record UpdateUserEmailCommand(Guid UserId, string NewEmail) : IRequest<Result>;
}