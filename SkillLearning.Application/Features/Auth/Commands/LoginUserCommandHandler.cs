using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SkillLearning.Application.Common.Configuration;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Common.Models;
using SkillLearning.Domain.Events;

namespace SkillLearning.Application.Features.Auth.Commands
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<AuthResultDto>>
    {
        private readonly IAuthService _authService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JwtSettings _jwtSettings;
        private readonly IUserRepository _userRepository;

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

        public async Task<Result<AuthResultDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByUsernameAsync(request.Username);

            if (user == null || !user.VerifyPassword(request.Password))
                return Result.Fail<AuthResultDto>("Nome de usuário ou senha inválidos.");

            var userDto = new UserDto(user.Id, user.Username, user.Email, user.Role);
            var claims = await _authService.GetUserClaims(userDto);
            var token = _authService.GenerateJwtToken(claims, _jwtSettings.Issuer);

            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";

            var userLoginEvent = new UserLoginEvent(user.Id, user.Username, user.Email, DateTime.UtcNow, ipAddress, userAgent);
            await _eventPublisher.PublishAsync(userLoginEvent);

            var authResult = new AuthResultDto(token, DateTime.UtcNow.AddMinutes(30));
            return Result.Ok(authResult);
        }
    }
}