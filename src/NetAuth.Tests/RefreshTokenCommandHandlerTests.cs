using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetAuth.Data.Configuration;
using NetAuth.Data.Models;
using NetAuth.UseCases.Commands;
using NetAuth.UseCases.Services;
using System.Collections.Generic;

namespace NetAuth.Tests;

public class RefreshTokenCommandHandlerTests
{
    private static AuthDbContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AuthDbContext(options);
    }

    [Fact]
    public async Task Handle_Returns_New_Tokens_When_RefreshToken_Valid()
    {
        await using var context = BuildContext();
        var user = new User { Id = Guid.NewGuid(), Username = "alice", PasswordHash = "hash" };
        context.Users.Add(user);
        var refresh = new RefreshToken { Token = "oldtoken", UserId = user.Id, ExpiresAt = DateTime.UtcNow.AddDays(1) };
        context.RefreshTokens.Add(refresh);
        await context.SaveChangesAsync();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "Jwt:Secret", "0123456789abcdef0123456789abcdef" } })
            .Build();
        var tokenService = new TokenService(config, context);
        var handler = new RefreshTokenCommandHandler(context, tokenService);

        var result = await handler.Handle(new RefreshTokenCommand("oldtoken"), CancellationToken.None);

        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBe("oldtoken");
    }

    [Fact]
    public async Task Handle_Throws_When_RefreshToken_Invalid()
    {
        await using var context = BuildContext();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "Jwt:Secret", "0123456789abcdef0123456789abcdef" } })
            .Build();
        var tokenService = new TokenService(config, context);
        var handler = new RefreshTokenCommandHandler(context, tokenService);

        var act = () => handler.Handle(new RefreshTokenCommand("missing"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
