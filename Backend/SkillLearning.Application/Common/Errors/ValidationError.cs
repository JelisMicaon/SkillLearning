using FluentResults;

namespace SkillLearning.Application.Common.Errors
{
    public class ValidationError(string message) : Error(message)
    {
    }
}