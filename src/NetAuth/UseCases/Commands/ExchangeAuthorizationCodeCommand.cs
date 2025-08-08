using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NetAuth.Data.Configuration;

namespace NetAuth.UseCases.Commands;

public record ExchangeAuthorizationCodeCommand(string Code, string CodeVerifier, string ClientId, string RedirectUri) : IRequest<string>;

public class ExchangeAuthorizationCodeCommandHandler : IRequestHandler<ExchangeAuthorizationCodeCommand, string>
{
    private readonly AuthDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public ExchangeAuthorizationCodeCommandHandler(AuthDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<string> Handle(ExchangeAuthorizationCodeCommand request, CancellationToken cancellationToken)
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
        await _dbContext.SaveChangesAsync(cancellationToken);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Username) }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
