using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillLearning.Api.Controllers.Common;
using SkillLearning.Application.Features.Auth.CheckUserExistsUseCase;
using SkillLearning.Application.Features.Auth.LoginUserUseCase;
using SkillLearning.Application.Features.Auth.RefreshTokenUseCase;
using SkillLearning.Application.Features.Auth.RegisterUserUseCase;

namespace SkillLearning.Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IMediator mediator) : ApiControllerBase
    {
        [HttpGet("exists")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckUserExists([FromQuery] string username, [FromQuery] string email)
        {
            var query = new CheckUserExistsQuery(username, email);
            var result = await mediator.Send(query);
            return HandleResult(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
        {
            var result = await mediator.Send(command);
            return HandleResult(result);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
        {
            var result = await mediator.Send(command);
            return HandleResult(result);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            var result = await mediator.Send(command);
            if (result.IsSuccess)
                return Ok(new { Message = "Usuário registrado com sucesso!" });

            return HandleResult(result);
        }
    }
}