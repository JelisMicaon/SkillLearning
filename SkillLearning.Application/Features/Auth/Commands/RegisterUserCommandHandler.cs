using FluentResults;
using MediatR;
using SkillLearning.Application.Common.Errors;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Common.Models;
using SkillLearning.Domain.Entities;
using SkillLearning.Domain.Events;

namespace SkillLearning.Application.Features.Auth.Commands
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result>
    {
        private readonly ICacheService _cacheService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IUserRepository _userRepository;

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IEventPublisher eventPublisher,
            ICacheService cacheService)
        {
            _userRepository = userRepository;
            _eventPublisher = eventPublisher;
            _cacheService = cacheService;
        }

        public async Task<Result> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var userExists = await _userRepository.DoesUserExistAsync(request.Username, request.Email);
            if (userExists)
                return Result.Fail(new ValidationError("O nome de usuário ou e-mail já está em uso."));

            var user = new User(request.Username, request.Email, request.Password);

            await _userRepository.AddUserAsync(user);

            var userDto = new UserDto(user.Id, user.Username, user.Email, user.Role);

            var userIdKey = $"username:{user.Username}";
            var userKey = $"user:{user.Id}";
            var emailKey = $"email:{user.Email}";

            await _cacheService.SetAsync(userIdKey, user.Id, TimeSpan.FromMinutes(10));
            await _cacheService.SetAsync(emailKey, user.Id, TimeSpan.FromMinutes(10));
            await _cacheService.SetAsync(userKey, userDto, TimeSpan.FromMinutes(30));

            var userRegisteredEvent = new UserRegisteredEvent(user.Id, user.Username, user.Email, user.CreatedAt);
            await _eventPublisher.PublishAsync(userRegisteredEvent);

            return Result.Ok();
        }
    }
}