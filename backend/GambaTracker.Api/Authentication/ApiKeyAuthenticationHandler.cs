using System.Security.Claims;
using System.Text.Encodings.Web;
using GambaTracker.Core.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace GambaTracker.Api.Authentication;

// Authenticates requests that send an "X-Api-Key" header, as the key's owner.
// Sets the same NameIdentifier claim as the JWT, so User.GetUserId() works for both.
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-Api-Key";

    private readonly IApiKeyRepository _apiKeyRepository;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyRepository apiKeyRepository)
        : base(options, logger, encoder)
    {
        _apiKeyRepository = apiKeyRepository;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var values) || string.IsNullOrWhiteSpace(values.ToString()))
        {
            // No key sent: let another scheme (JWT) try
            return AuthenticateResult.NoResult();
        }

        var apiKey = await _apiKeyRepository.GetApiKeyByHashAsync(ApiKeyHasher.Hash(values.ToString().Trim()));
        if (apiKey == null)
        {
            return AuthenticateResult.Fail("Invalid API key.");
        }

        await _apiKeyRepository.UpdateLastUsedAsync(apiKey.Id, DateTime.UtcNow);

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, apiKey.UserId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        return AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName));
    }
}
