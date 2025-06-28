using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillLearning.Application.Features.Auth.Commands;

namespace SkillLearning.Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(command);

            if (!result)
            {
                return BadRequest(new { Message = "Registration failed. Username or email might already be in use." });
            }

            return Ok(new { Message = "User registered successfully!" });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authResult = await _mediator.Send(command);

            if (authResult == null)
            {
                return Unauthorized(new { Message = "Invalid username or password." });
            }

            return Ok(authResult);
        }
    }
}