using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillLearning.Application.Common.Errors;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Common.Models;

namespace SkillLearning.Application.Features.Auth.Queries
{
    public class GetUserByUsernameQueryHandler(ICacheService cacheService, IReadDbContext readContext) : IRequestHandler<GetUserByUsernameQuery, Result<UserDto>>
    {
        public async Task<Result<UserDto>> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
        {
            var userIdCacheKey = $"username:{request.Username}";
            var userId = await cacheService.GetAsync<Guid?>(userIdCacheKey);

            if (userId.HasValue)
            {
                var userCacheKey = $"user:{userId.Value}";
                var cachedUserDto = await cacheService.GetAsync<UserDto>(userCacheKey);
                if (cachedUserDto != null)
                    return Result.Ok(cachedUserDto);
            }

            var user = await readContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);
            if (user == null)
                return Result.Fail(new NotFoundError("Usuário não encontrado."));

            var userDto = new UserDto(user.Id, user.Username, user.Email, user.Role);

            await cacheService.SetAsync(userIdCacheKey, user.Id, TimeSpan.FromMinutes(10));
            string userKey = $"user:{user.Id}";
            await cacheService.SetAsync(userKey, userDto, TimeSpan.FromMinutes(30));

            return Result.Ok(userDto);
        }
    }
}