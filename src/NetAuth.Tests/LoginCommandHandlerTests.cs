using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetAuth.Data.Configuration;
using NetAuth.Data.Models;
using NetAuth.UseCases.Commands;
using NSubstitute;

namespace NetAuth.Tests;

public class LoginCommandHandlerTests
{
    private static AuthDbContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AuthDbContext(options);
    }

    [Fact]
    public async Task Handle_Returns_Token_For_Valid_Credentials()
    {
        await using var context = BuildContext();
        var user = new User { Id = Guid.NewGuid(), Username = "alice", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password") };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        IConfiguration configuration = Substitute.For<IConfiguration>();
        configuration["Jwt:Secret"].Returns("0123456789abcdef0123456789abcdef");

        var handler = new LoginCommandHandler(context, configuration);
        var command = new LoginCommand("alice", "password");

        var token = await handler.Handle(command, CancellationToken.None);

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_Throws_For_Invalid_Credentials()
    {
        await using var context = BuildContext();
        IConfiguration configuration = Substitute.For<IConfiguration>();
        configuration["Jwt:Secret"].Returns("0123456789abcdef0123456789abcdef");

        var handler = new LoginCommandHandler(context, configuration);
        var command = new LoginCommand("bob", "wrong");

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
