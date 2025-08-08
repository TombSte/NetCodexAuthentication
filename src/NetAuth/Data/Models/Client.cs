namespace NetAuth.Data.Models;

public class Client
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ClientId { get; set; } = string.Empty;
    public string? RedirectUri { get; set; }
}
