using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace SkillLearning.Infrastructure.Controllers.Common
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsFailed)
            {
                var errorResponse = new { Errors = result.Errors.Select(e => e.Message) };
                return BadRequest(errorResponse);
            }

            return Ok(result.Value);
        }
    }
}
