using MediatR;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Domain.Entities;
using SkillLearning.Domain.Enums;
using SkillLearning.Domain.Events;

namespace SkillLearning.Application.Features.Auth.Commands
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEventPublisher _eventPublisher;

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IEventPublisher eventPublisher)
        {
            _userRepository = userRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<bool> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            if (await _userRepository.GetUserByUsernameAsync(request.Username) != null ||
                await _userRepository.GetUserByEmailAsync(request.Email) != null)
            {
                return false;
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = UserRole.User
            };

            await _userRepository.AddUserAsync(user);

            var userRegisteredEvent = new UserRegisteredEvent(
                user.Id,
                user.Username,
                user.Email
            );

            await _eventPublisher.PublishAsync(userRegisteredEvent);

            return true;
        }
    }
}