using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillLearning.Application.Common.Interfaces;

namespace SkillLearning.Application.Features.Auth.CheckUserExistsUseCase
{
    public class CheckUserExistsQueryHandler(IReadDbContext readContext, ICacheService cacheService) : IRequestHandler<CheckUserExistsQuery, Result<bool>>
    {
        public async Task<Result<bool>> Handle(CheckUserExistsQuery request, CancellationToken cancellationToken)
        {
            var usernameKey = $"username:{request.Username}";
            var emailKey = $"email:{request.Email}";

            var userIdByUsername = await cacheService.GetAsync<Guid?>(usernameKey);
            if (userIdByUsername.HasValue)
                return Result.Ok(true);

            var userIdByEmail = await cacheService.GetAsync<Guid?>(emailKey);
            if (userIdByEmail.HasValue)
                return Result.Ok(true);

            var exists = await readContext.Users.AsNoTracking().AnyAsync(u => u.Username == request.Username || u.Email == request.Email, cancellationToken);
            return Result.Ok(exists);
        }
    }
}