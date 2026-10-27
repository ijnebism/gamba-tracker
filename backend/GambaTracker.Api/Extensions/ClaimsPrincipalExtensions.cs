using System.Security.Claims;

namespace GambaTracker.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    // Reads the caller's user id from the JWT (set by TokenService as ClaimTypes.NameIdentifier).
    // Only call this from [Authorize] endpoints, where the claim is guaranteed to exist.
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user has no NameIdentifier claim.");

        return Guid.Parse(userIdClaim);
    }
}
