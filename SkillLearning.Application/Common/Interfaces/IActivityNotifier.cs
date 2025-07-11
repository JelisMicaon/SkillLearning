namespace SkillLearning.Application.Common.Interfaces
{
    public interface IActivityNotifier
    {
        Task NotifyNewUserRegistered(string username);

        Task NotifyUserLoggedIn(string username);
    }
}