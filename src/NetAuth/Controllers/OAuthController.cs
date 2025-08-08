using MediatR;
using Microsoft.AspNetCore.Mvc;
using NetAuth.UseCases.Commands;

namespace NetAuth.Controllers;

[ApiController]
[Route("oauth2")]
public class OAuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public OAuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("authorize")]
    public async Task<IActionResult> Authorize([FromBody] AuthorizeRequest request)
    {
        var code = await _mediator.Send(new GenerateAuthorizationCodeCommand(request.Username, request.Password, request.ClientId, request.RedirectUri, request.CodeChallenge, request.CodeChallengeMethod));
        return Ok(new { code });
    }

    [HttpPost("token")]
    public async Task<IActionResult> Token([FromBody] TokenRequest request)
    {
        var result = await _mediator.Send(new ExchangeAuthorizationCodeCommand(request.Code, request.CodeVerifier, request.ClientId, request.RedirectUri));
        return Ok(new { access_token = result.AccessToken, refresh_token = result.RefreshToken });
    }
}

public record AuthorizeRequest(string Username, string Password, string ClientId, string RedirectUri, string CodeChallenge, string CodeChallengeMethod);
public record TokenRequest(string Code, string CodeVerifier, string ClientId, string RedirectUri);
