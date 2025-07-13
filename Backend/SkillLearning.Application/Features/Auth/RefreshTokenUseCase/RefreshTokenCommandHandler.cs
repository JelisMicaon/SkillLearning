using FluentResults;
using MediatR;
using Microsoft.Extensions.Options;
using SkillLearning.Application.Common.Configuration;
using SkillLearning.Application.Common.Errors;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Common.Models;
using SkillLearning.Domain.Entities;
using System.Security.Claims;

namespace SkillLearning.Application.Features.Auth.RefreshTokenUseCase
{
    public class RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IAuthService authService,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettingsOptions) : IRequestHandler<RefreshTokenCommand, Result<AuthResultDto>>
    {
        public async Task<Result<AuthResultDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = authService.GetPrincipalFromExpiredToken(request.AccessToken);
            var userIdString = principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return Result.Fail(new AuthenticationError("Token de acesso inválido ou malformado."));

            var user = await userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return Result.Fail(new AuthenticationError("Usuário associado ao token não encontrado."));

            var oldRefreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken);
            if (oldRefreshToken == null || !oldRefreshToken.IsActive)
                return Result.Fail(new AuthenticationError("Refresh token inválido ou expirado."));

            oldRefreshToken.Revoke();

            var userDto = new UserDto(user.Id, user.Username, user.Email, user.Role);
            var claims = await authService.GetUserClaims(userDto);
            var newAccessToken = authService.GenerateJwtToken(claims, jwtSettingsOptions.Value.Issuer);

            var newRefreshToken = new RefreshToken(TimeSpan.FromDays(7));
            user.AddRefreshToken(newRefreshToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var authResult = new AuthResultDto(newAccessToken, newRefreshToken.Token);
            return Result.Ok(authResult);
        }
    }
}