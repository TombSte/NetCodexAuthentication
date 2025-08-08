using Microsoft.EntityFrameworkCore;
using NetAuth.Data.Models;

namespace NetAuth.Data.Configuration;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<ClientSecret> ClientSecrets => Set<ClientSecret>();
    public DbSet<AuthorizationCode> AuthorizationCodes => Set<AuthorizationCode>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
}
