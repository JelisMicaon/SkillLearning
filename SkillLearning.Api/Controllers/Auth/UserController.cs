using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillLearning.Api.Contracts;
using SkillLearning.Api.Controllers.Common;
using SkillLearning.Application.Features.Auth.Queries;
using SkillLearning.Application.Features.Users.Commands;

namespace SkillLearning.Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController(IMediator mediator) : ApiControllerBase
    {
        [HttpGet("{username}")]
        public async Task<IActionResult> GetByUsername(string username)
        {
            var query = new GetUserByUsernameQuery(username);
            var result = await mediator.Send(query);
            return HandleResult(result);
        }

        [HttpPut("{id}/email")]
        public async Task<IActionResult> UpdateEmail(Guid id, [FromBody] UpdateEmailRequest request)
        {
            var command = new UpdateUserEmailCommand(id, request.Email);
            var result = await mediator.Send(command);
            return HandleResult(result);
        }
    }
}