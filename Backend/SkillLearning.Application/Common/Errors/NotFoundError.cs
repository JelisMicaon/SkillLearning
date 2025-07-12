using FluentResults;

namespace SkillLearning.Application.Common.Errors
{
    public class NotFoundError(string message) : Error(message)
    {
    }
}