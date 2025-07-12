using Microsoft.AspNetCore.SignalR;
using SkillLearning.Api.Hubs;
using SkillLearning.Application.Common.Interfaces;

namespace SkillLearning.Api.Services
{
    public class SignalRActivityNotifier(IHubContext<ActivityHub> hubContext) : IActivityNotifier
    {
        public Task NotifyNewUserRegistered(string username)
            => hubContext.Clients.All.SendAsync("NewUserRegistered", username);

        public Task NotifyUserLoggedIn(string username)
            => hubContext.Clients.All.SendAsync("UserLoggedIn", username);
    }
}