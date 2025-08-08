using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NetAuth.Data.Configuration;
using NetAuth.Data.Models;

namespace NetAuth.UseCases.Services;

public interface ITokenService
{
    Task<string> GenerateAccessToken(User user, CancellationToken cancellationToken);
    Task<string> GenerateRefreshToken(Guid userId, Guid? clientId, CancellationToken cancellationToken);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly AuthDbContext _dbContext;

    public TokenService(IConfiguration configuration, AuthDbContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
    }

    public Task<string> GenerateAccessToken(User user, CancellationToken cancellationToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Username) }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Task.FromResult(tokenHandler.WriteToken(token));
    }

    public async Task<string> GenerateRefreshToken(Guid userId, Guid? clientId, CancellationToken cancellationToken)
    {
        var token = Guid.NewGuid().ToString("N");
        var refresh = new RefreshToken
        {
            Token = token,
            UserId = userId,
            ClientId = clientId,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        _dbContext.RefreshTokens.Add(refresh);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return token;
    }
}
