namespace NetAuth.Data.Models;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid? ClientId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
