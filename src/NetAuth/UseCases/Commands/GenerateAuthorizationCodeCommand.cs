using MediatR;
using Microsoft.EntityFrameworkCore;
using NetAuth.Data.Configuration;
using NetAuth.Data.Models;

namespace NetAuth.UseCases.Commands;

public record GenerateAuthorizationCodeCommand(string Username, string Password, string ClientId, string RedirectUri, string CodeChallenge, string CodeChallengeMethod) : IRequest<string>;

public class GenerateAuthorizationCodeCommandHandler : IRequestHandler<GenerateAuthorizationCodeCommand, string>
{
    private readonly AuthDbContext _dbContext;

    public GenerateAuthorizationCodeCommandHandler(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> Handle(GenerateAuthorizationCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == request.Username, cancellationToken);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var client = await _dbContext.Clients.SingleOrDefaultAsync(c => c.ClientId == request.ClientId && c.RedirectUri == request.RedirectUri, cancellationToken);
        if (client is null)
        {
            throw new InvalidOperationException("Invalid client");
        }

        var code = Guid.NewGuid().ToString("N");
        var authorization = new AuthorizationCode
        {
            Code = code,
            UserId = user.Id,
            ClientId = client.Id,
            CodeChallenge = request.CodeChallenge,
            CodeChallengeMethod = request.CodeChallengeMethod,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        _dbContext.AuthorizationCodes.Add(authorization);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return code;
    }
}
