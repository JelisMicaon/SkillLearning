using MediatR;
using SkillLearning.Application.Common.Interfaces;

namespace SkillLearning.Application.Features.Auth.Queries
{
    public class CheckUserExistsQueryHandler : IRequestHandler<CheckUserExistsQuery, bool>
    {
        private readonly ICacheService _cacheService;
        private readonly IUserRepository _userRepository;

        public CheckUserExistsQueryHandler(IUserRepository userRepository, ICacheService cacheService)
        {
            _userRepository = userRepository;
            _cacheService = cacheService;
        }

        public async Task<bool> Handle(CheckUserExistsQuery request, CancellationToken cancellationToken)
        {
            var usernameKey = $"username:{request.Username}";
            var emailKey = $"email:{request.Email}";

            var userIdByUsername = await _cacheService.GetAsync<Guid?>(usernameKey);
            if (userIdByUsername.HasValue)
                return true;

            var userIdByEmail = await _cacheService.GetAsync<Guid?>(emailKey);
            if (userIdByEmail.HasValue)
                return true;

            return await _userRepository.DoesUserExistAsync(request.Username, request.Email);
        }
    }
}