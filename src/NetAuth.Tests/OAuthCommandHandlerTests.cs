using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetAuth.Data.Configuration;
using NetAuth.Data.Models;
using NetAuth.UseCases.Commands;
using NSubstitute;
using System.Security.Cryptography;
using System.Text;

namespace NetAuth.Tests;

public class OAuthCommandHandlerTests
{
    private static AuthDbContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AuthDbContext(options);
    }

    [Fact]
    public async Task ExchangeAuthorizationCode_Returns_Token_When_Valid()
    {
        await using var context = BuildContext();
        var user = new User { Id = Guid.NewGuid(), Username = "alice", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password") };
        var client = new Client { Id = Guid.NewGuid(), ClientId = "spa", RedirectUri = "https://app/callback" };
        var verifier = "verifier";
        var challenge = ToBase64Url(SHA256.HashData(Encoding.UTF8.GetBytes(verifier)));
        context.Users.Add(user);
        context.Clients.Add(client);
        context.AuthorizationCodes.Add(new AuthorizationCode
        {
            Code = "code123",
            UserId = user.Id,
            ClientId = client.Id,
            CodeChallenge = challenge,
            CodeChallengeMethod = "S256",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        });
        await context.SaveChangesAsync();

        IConfiguration configuration = Substitute.For<IConfiguration>();
        configuration["Jwt:Secret"].Returns("0123456789abcdef0123456789abcdef");

        var handler = new ExchangeAuthorizationCodeCommandHandler(context, configuration);
        var token = await handler.Handle(new ExchangeAuthorizationCodeCommand("code123", verifier, "spa", "https://app/callback"), CancellationToken.None);

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ExchangeAuthorizationCode_Throws_For_Invalid_Verifier()
    {
        await using var context = BuildContext();
        var user = new User { Id = Guid.NewGuid(), Username = "alice", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password") };
        var client = new Client { Id = Guid.NewGuid(), ClientId = "spa", RedirectUri = "https://app/callback" };
        var verifier = "verifier";
        var challenge = ToBase64Url(SHA256.HashData(Encoding.UTF8.GetBytes(verifier)));
        context.Users.Add(user);
        context.Clients.Add(client);
        context.AuthorizationCodes.Add(new AuthorizationCode
        {
            Code = "code123",
            UserId = user.Id,
            ClientId = client.Id,
            CodeChallenge = challenge,
            CodeChallengeMethod = "S256",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        });
        await context.SaveChangesAsync();

        IConfiguration configuration = Substitute.For<IConfiguration>();
        configuration["Jwt:Secret"].Returns("0123456789abcdef0123456789abcdef");

        var handler = new ExchangeAuthorizationCodeCommandHandler(context, configuration);
        var act = () => handler.Handle(new ExchangeAuthorizationCodeCommand("code123", "wrong", "spa", "https://app/callback"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static string ToBase64Url(byte[] input) =>
        Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
