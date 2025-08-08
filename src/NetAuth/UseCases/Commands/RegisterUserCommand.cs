using MediatR;
using NetAuth.Data.Configuration;
using NetAuth.Data.Models;

namespace NetAuth.UseCases.Commands;

public record RegisterUserCommand(string Username, string Password) : IRequest<Guid>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly AuthDbContext _dbContext;

    public RegisterUserCommandHandler(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
