public class ApiKey
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;

    // SHA-256 of the full key. The key itself is never stored
    public string KeyHash { get; set; } = null!;
    // First characters of the key, so users can tell their keys apart
    public string Prefix { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}
