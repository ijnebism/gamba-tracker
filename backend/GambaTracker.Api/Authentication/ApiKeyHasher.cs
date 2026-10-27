using System.Security.Cryptography;
using System.Text;

namespace GambaTracker.Api.Authentication;

public static class ApiKeyHasher
{
    private const string KeyPrefix = "gt_";
    // "gt_" plus the first 8 random characters, shown in key lists
    public const int DisplayPrefixLength = 11;

    // 32 random bytes, URL-safe base64, e.g. "gt_Xk3...". High entropy, so a plain SHA-256 is enough to store it
    public static string GenerateKey() =>
        KeyPrefix + Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static string Hash(string key) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
}
