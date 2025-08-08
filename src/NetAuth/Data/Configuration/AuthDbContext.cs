using Microsoft.EntityFrameworkCore;
using NetAuth.Data.Models;

namespace NetAuth.Data.Configuration;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
}
