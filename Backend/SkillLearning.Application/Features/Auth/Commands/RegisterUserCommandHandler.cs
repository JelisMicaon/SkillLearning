using FluentResults;
using MediatR;
using SkillLearning.Application.Common.Errors;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Domain.Entities;
using SkillLearning.Domain.Events;

namespace SkillLearning.Application.Features.Auth.Commands
{
    public class RegisterUserCommandHandler(
         IUserRepository userRepository,
         IEventPublisher eventPublisher,
         IUnitOfWork unitOfWork,
         IActivityNotifier activityNotifier) : IRequestHandler<RegisterUserCommand, Result>
    {
        public async Task<Result> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var userExists = await userRepository.DoesUserExistAsync(request.Username, request.Email);
            if (userExists)
                return Result.Fail(new ValidationError("O nome de usuário ou e-mail já está em uso."));

            var user = new User(request.Username, request.Email, request.Password);

            userRepository.AddUser(user);

            var userRegisteredEvent = new UserRegisteredEvent(user.Id, user.Username, user.Email, user.CreatedAt);
            await eventPublisher.PublishAsync(userRegisteredEvent);
            await activityNotifier.NotifyNewUserRegistered(user.Username);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}