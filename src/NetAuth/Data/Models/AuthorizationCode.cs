namespace NetAuth.Data.Models;

public class AuthorizationCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid ClientId { get; set; }
    public string CodeChallenge { get; set; } = string.Empty;
    public string CodeChallengeMethod { get; set; } = "S256";
    public DateTime ExpiresAt { get; set; }
}
