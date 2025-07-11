using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SkillLearning.Application.Common.Configuration;
using SkillLearning.Application.Common.Errors;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Common.Models;
using SkillLearning.Domain.Entities;
using SkillLearning.Domain.Events;

namespace SkillLearning.Application.Features.Auth.Commands
{
    public class LoginUserCommandHandler(
        IUserRepository userRepository,
        IAuthService authService,
        IOptions<JwtSettings> jwtSettingsOptions,
        IEventPublisher eventPublisher,
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork,
        IActivityNotifier activityNotifier) : IRequestHandler<LoginUserCommand, Result<AuthResultDto>>
    {

        public async Task<Result<AuthResultDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetUserByUsernameAsync(request.Username);

            if (user == null || !user.VerifyPassword(request.Password))
                return Result.Fail(new AuthenticationError("Nome de usuário ou senha inválidos."));

            var userDto = new UserDto(user.Id, user.Username, user.Email, user.Role);
            var claims = await authService.GetUserClaims(userDto);
            var accessToken = authService.GenerateJwtToken(claims, jwtSettingsOptions.Value.Issuer);

            var refreshToken = new RefreshToken(TimeSpan.FromDays(7));
            refreshToken.UserId = user.Id;
            user.AddRefreshToken(refreshToken);
            userRepository.AddRefreshToken(refreshToken);

            var ipAddress = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
            var userLoginEvent = new UserLoginEvent(user.Id, user.Username, user.Email, DateTime.UtcNow, ipAddress, userAgent);
            await eventPublisher.PublishAsync(userLoginEvent);
            await activityNotifier.NotifyUserLoggedIn(user.Username);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var authResult = new AuthResultDto(accessToken, refreshToken.Token);
            return Result.Ok(authResult);
        }
    }
}