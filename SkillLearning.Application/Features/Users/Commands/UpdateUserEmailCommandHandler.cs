using FluentResults;
using MediatR;
using SkillLearning.Application.Common.Errors;
using SkillLearning.Application.Common.Interfaces;

namespace SkillLearning.Application.Features.Users.Commands
{
    public class UpdateUserEmailCommandHandler(IUserRepository userRepository, ICacheService cacheService, IUnitOfWork unitOfWork) : IRequestHandler<UpdateUserEmailCommand, Result>
    {
        public async Task<Result> Handle(UpdateUserEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
                return Result.Fail(new NotFoundError("Usuário não encontrado."));

            if (!user.Email.Equals(request.NewEmail, StringComparison.OrdinalIgnoreCase))
            {
                var emailInUse = await userRepository.IsEmailInUseAsync(request.NewEmail);
                if (emailInUse)
                    return Result.Fail(new ValidationError("O endereço de e-mail informado já está em uso."));
            }

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

            await unitOfWork.SaveChangesAsync(cancellationToken);

            await cacheService.RemoveAsync($"user:{user.Id}");
            await cacheService.RemoveAsync($"username:{user.Username}");
            await cacheService.RemoveAsync($"email:{oldEmail}");
            await cacheService.RemoveAsync($"email:{request.NewEmail}");

            return Result.Ok();
        }
    }
}