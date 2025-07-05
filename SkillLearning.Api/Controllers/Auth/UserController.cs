using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillLearning.Application.Features.Auth.Queries;
using SkillLearning.Infrastructure.Controllers.Common;

namespace SkillLearning.Infrastructure.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ApiControllerBase
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
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }
    }
}