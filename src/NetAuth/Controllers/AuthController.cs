using MediatR;
using Microsoft.AspNetCore.Mvc;
using NetAuth.UseCases.Commands;

namespace NetAuth.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { UserId = id });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { access_token = result.AccessToken, refresh_token = result.RefreshToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { access_token = result.AccessToken, refresh_token = result.RefreshToken });
    }
}
