using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillLearning.Application.Features.Auth.Queries;

namespace SkillLearning.Infrastructure.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetByUsername(string username)
        {
            var query = new GetUserByUsernameQuery(username);
            var userDto = await _mediator.Send(query);

            if (userDto is null)
            {
                return NotFound(new { Message = "User not found." });
            }

            return Ok(userDto);
        }
    }
}