using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetAuth.Data.Configuration;
using NetAuth.UseCases.Models;
using NetAuth.UseCases.Services;

namespace NetAuth.UseCases.Commands;

public record ExchangeAuthorizationCodeCommand(string Code, string CodeVerifier, string ClientId, string RedirectUri) : IRequest<AuthResult>;

public class ExchangeAuthorizationCodeCommandHandler : IRequestHandler<ExchangeAuthorizationCodeCommand, AuthResult>
{
    private readonly AuthDbContext _dbContext;
    private readonly ITokenService _tokenService;

    public ExchangeAuthorizationCodeCommandHandler(AuthDbContext dbContext, ITokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<AuthResult> Handle(ExchangeAuthorizationCodeCommand request, CancellationToken cancellationToken)
    {
        var client = await _dbContext.Clients.SingleOrDefaultAsync(c => c.ClientId == request.ClientId && c.RedirectUri == request.RedirectUri, cancellationToken);
        if (client is null)
        {
            throw new InvalidOperationException("Invalid client");
        }

        var authorization = await _dbContext.AuthorizationCodes.SingleOrDefaultAsync(c => c.Code == request.Code && c.ClientId == client.Id, cancellationToken);
        if (authorization is null || authorization.ExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Invalid authorization code");
        }

        var expectedChallenge = authorization.CodeChallengeMethod switch
        {
            "S256" => Base64UrlEncoder.Encode(SHA256.HashData(Encoding.UTF8.GetBytes(request.CodeVerifier))),
            _ => request.CodeVerifier
        };

        if (expectedChallenge != authorization.CodeChallenge)
        {
            throw new InvalidOperationException("Invalid code verifier");
        }

        var user = await _dbContext.Users.SingleAsync(u => u.Id == authorization.UserId, cancellationToken);

        _dbContext.AuthorizationCodes.Remove(authorization);

        var accessToken = await _tokenService.GenerateAccessToken(user, cancellationToken);
        var refreshToken = await _tokenService.GenerateRefreshToken(user.Id, client.Id, cancellationToken);
        return new AuthResult(accessToken, refreshToken);
    }
}
