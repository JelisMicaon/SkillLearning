using FluentResults;

namespace SkillLearning.Application.Common.Errors
{
    public class AuthenticationError(string message) : Error(message)
    {
    }
}