using MediatR;
using Microsoft.Extensions.Options;
using SkillLearning.Application.Common.Configuration;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Common.Models;

namespace SkillLearning.Application.Features.Auth.Commands
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResultDto?>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly JwtSettings _jwtSettings;

        public LoginUserCommandHandler(IUserRepository userRepository, IAuthService authService, IOptions<JwtSettings> jwtSettingsOptions)
        {
            _userRepository = userRepository;
            _authService = authService;
            _jwtSettings = jwtSettingsOptions.Value;
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

            return new AuthResultDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(30)
            };
        }
    }
}