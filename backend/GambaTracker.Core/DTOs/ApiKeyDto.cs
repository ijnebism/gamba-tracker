using System.ComponentModel.DataAnnotations;

namespace GambaTracker.Core.DTOs
{
    // e.g. { "name": "Skyblock mod" }
    public record CreateApiKeyRequest([Required, StringLength(100)] string Name);

    public record ApiKeyResponse(Guid Id, string Name, string Prefix, DateTime CreatedAt, DateTime? LastUsedAt);

    // Only returned once, when the key is created. Key is the full secret to paste into the mod or script
    public record CreatedApiKeyResponse(Guid Id, string Name, string Prefix, DateTime CreatedAt, string Key);
}
