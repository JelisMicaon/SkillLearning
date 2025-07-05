using FluentResults;
using MediatR;
using SkillLearning.Application.Common.Errors;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Common.Models;

namespace SkillLearning.Application.Features.Auth.Queries
{
    public class GetUserByUsernameQueryHandler : IRequestHandler<GetUserByUsernameQuery, Result<UserDto>>
    {
        private readonly ICacheService _cacheService;
        private readonly IUserRepository _userRepository;

        public GetUserByUsernameQueryHandler(ICacheService cacheService, IUserRepository userRepository)
        {
            _cacheService = cacheService;
            _userRepository = userRepository;
        }

        public async Task<Result<UserDto>> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
        {
            var userIdCacheKey = $"username:{request.Username}";
            var userId = await _cacheService.GetAsync<Guid?>(userIdCacheKey);

            if (userId.HasValue)
            {
                var userCacheKey = $"user:{userId.Value}";
                var cachedUserDto = await _cacheService.GetAsync<UserDto>(userCacheKey);
                if (cachedUserDto != null)
                    return Result.Ok(cachedUserDto);
            }

            var user = await _userRepository.GetUserByUsernameAsync(request.Username);
            if (user == null)
                return Result.Fail(new NotFoundError("Usuário não encontrado."));

            var userDto = new UserDto(user.Id, user.Username, user.Email, user.Role);

            await _cacheService.SetAsync(userIdCacheKey, user.Id, TimeSpan.FromMinutes(10));
            string userKey = $"user:{user.Id}";
            await _cacheService.SetAsync(userKey, userDto, TimeSpan.FromMinutes(30));

            return Result.Ok(userDto);
        }
    }
}