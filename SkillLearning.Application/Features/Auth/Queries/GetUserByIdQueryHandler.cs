using MediatR;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Common.Models;

namespace SkillLearning.Application.Features.Auth.Queries
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
    {
        private readonly ICacheService _cacheService;
        private readonly IUserRepository _userRepository;

        public GetUserByIdQueryHandler(ICacheService cacheService, IUserRepository userRepository)
        {
            _cacheService = cacheService;
            _userRepository = userRepository;
        }

        public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"user:{request.Id}";
            var userDto = await _cacheService.GetAsync<UserDto>(cacheKey);

            if (userDto is not null)
                return userDto;

            var user = await _userRepository.GetUserByIdAsync(request.Id);

            if (user is not null)
            {
                userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    PasswordHash = user.PasswordHash
                };
                await _cacheService.SetAsync(cacheKey, userDto, TimeSpan.FromMinutes(30));
            }

            return userDto;
        }
    }
}