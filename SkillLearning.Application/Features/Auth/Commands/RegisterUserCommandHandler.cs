using MediatR;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Common.Models;
using SkillLearning.Application.Features.Auth.Queries;
using SkillLearning.Domain.Entities;
using SkillLearning.Domain.Enums;
using SkillLearning.Domain.Events;

namespace SkillLearning.Application.Features.Auth.Commands
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, bool>
    {
        private readonly IMediator _mediator;
        private readonly IUserRepository _userRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheService _cacheService;

        public RegisterUserCommandHandler(
            IMediator mediator,
            IUserRepository userRepository,
            IEventPublisher eventPublisher,
            ICacheService cacheService)
        {
            _mediator = mediator;
            _userRepository = userRepository;
            _eventPublisher = eventPublisher;
            _cacheService = cacheService;
        }

        public async Task<bool> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var userExists = await _mediator.Send(new CheckUserExistsQuery(request.Username, request.Email), cancellationToken);
            if (userExists)
                return false;

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = UserRole.User
            };

            await _userRepository.AddUserAsync(user);

            var userDto = new UserDto(
                user.Id,
                user.Username,
                user.Email,
                user.PasswordHash,
                user.Role
            );

            var userIdKey = $"username:{user.Username}";
            var userKey = $"user:{user.Id}";
            var emailKey = $"email:{user.Email}";

            await _cacheService.SetAsync(userIdKey, user.Id, TimeSpan.FromMinutes(10));
            await _cacheService.SetAsync(emailKey, user.Id, TimeSpan.FromMinutes(10));
            await _cacheService.SetAsync(userKey, userDto, TimeSpan.FromMinutes(30));

            var userRegisteredEvent = new UserRegisteredEvent(user.Id, user.Username, user.Email, user.CreatedAt);
            await _eventPublisher.PublishAsync(userRegisteredEvent);

            return true;
        }
    }
}