using MediatR;
using Microsoft.EntityFrameworkCore;
using NetAuth.Data.Configuration;
using NetAuth.UseCases.Models;
using NetAuth.UseCases.Services;

namespace NetAuth.UseCases.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResult>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly AuthDbContext _dbContext;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(AuthDbContext dbContext, ITokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.RefreshTokens.SingleOrDefaultAsync(r => r.Token == request.RefreshToken, cancellationToken);
        if (existing is null || existing.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var user = await _dbContext.Users.SingleAsync(u => u.Id == existing.UserId, cancellationToken);
        _dbContext.RefreshTokens.Remove(existing);

        var accessToken = await _tokenService.GenerateAccessToken(user, cancellationToken);
        var refreshToken = await _tokenService.GenerateRefreshToken(user.Id, existing.ClientId, cancellationToken);
        return new AuthResult(accessToken, refreshToken);
    }
}
