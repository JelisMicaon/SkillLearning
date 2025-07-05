using FluentResults;
using Microsoft.AspNetCore.Mvc;
using SkillLearning.Application.Common.Errors;

namespace SkillLearning.Api.Controllers.Common
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsFailed)
                return HandleFailure(result);

            if (result.ValueOrDefault is null)
                return NotFound();

            return Ok(result.Value);
        }

        protected IActionResult HandleResult(Result result)
        {
            if (result.IsFailed)
                return HandleFailure(result);

            return NoContent();
        }

        private IActionResult HandleFailure(ResultBase result)
        {
            var firstError = result.Errors[0];

            var errorResponse = new
            {
                Title = "Um ou mais erros ocorreram.",
                Errors = result.Errors.Select(e => e.Message)
            };

            switch (firstError)
            {
                case AuthenticationError:
                    return Unauthorized(errorResponse);

                case NotFoundError:
                    return NotFound(errorResponse);

                case ValidationError:
                    return BadRequest(errorResponse);

                default:
                    return BadRequest(errorResponse);
            }
        }
    }
}