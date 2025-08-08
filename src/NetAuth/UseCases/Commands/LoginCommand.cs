using MediatR;
using Microsoft.EntityFrameworkCore;
using NetAuth.Data.Configuration;
using NetAuth.UseCases.Models;
using NetAuth.UseCases.Services;

namespace NetAuth.UseCases.Commands;

public record LoginCommand(string Username, string Password) : IRequest<AuthResult>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly AuthDbContext _dbContext;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(AuthDbContext dbContext, ITokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == request.Username, cancellationToken);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var accessToken = await _tokenService.GenerateAccessToken(user, cancellationToken);
        var refreshToken = await _tokenService.GenerateRefreshToken(user.Id, null, cancellationToken);
        return new AuthResult(accessToken, refreshToken);
    }
}
