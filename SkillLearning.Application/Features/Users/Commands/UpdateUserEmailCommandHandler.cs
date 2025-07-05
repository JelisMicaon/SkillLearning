using FluentResults;
using MediatR;
using SkillLearning.Application.Common.Errors;
using SkillLearning.Application.Common.Interfaces;

namespace SkillLearning.Application.Features.Users.Commands
{
    public class UpdateUserEmailCommandHandler : IRequestHandler<UpdateUserEmailCommand, Result>
    {
        private readonly ICacheService _cacheService;
        private readonly IUserRepository _userRepository;

        public UpdateUserEmailCommandHandler(IUserRepository userRepository, ICacheService cacheService)
        {
            _userRepository = userRepository;
            _cacheService = cacheService;
        }

        public async Task<Result> Handle(UpdateUserEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
                return Result.Fail(new NotFoundError("Usuário não encontrado."));

            var oldEmail = user.Email;

            try
            {
                user.ChangeEmail(request.NewEmail);
            }
            catch (ArgumentException ex)
            {
                var cleanErrorMessage = ex.Message.Split('(')[0].Trim();
                return Result.Fail(new ValidationError(cleanErrorMessage));
            }

            await _userRepository.UpdateUserAsync(user);

            await _cacheService.RemoveAsync($"user:{user.Id}");
            await _cacheService.RemoveAsync($"username:{user.Username}");
            await _cacheService.RemoveAsync($"email:{oldEmail}");
            await _cacheService.RemoveAsync($"email:{request.NewEmail}");

            return Result.Ok();
        }
    }
}