using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SkillLearning.Application.Common.Configuration;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Common.Models;
using SkillLearning.Domain.Events;

namespace SkillLearning.Application.Features.Auth.Commands
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResultDto?>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly JwtSettings _jwtSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginUserCommandHandler(
            IUserRepository userRepository,
            IAuthService authService,
            IOptions<JwtSettings> jwtSettingsOptions,
            IEventPublisher eventPublisher,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _authService = authService;
            _jwtSettings = jwtSettingsOptions.Value;
            _eventPublisher = eventPublisher;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResultDto?> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByUsernameAsync(request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return null;
            }

            var claims = await _authService.GetUserClaims(user);
            var token = _authService.GenerateJwtToken(claims, _jwtSettings.Issuer);
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";

            var userLoginEvent = new UserLoginEvent
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            await _eventPublisher.PublishAsync(userLoginEvent);

            return new AuthResultDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(30)
            };
        }
    }
}