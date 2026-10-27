using GambaTracker.Api.Authentication;
using GambaTracker.Core.DTOs;
using GambaTracker.Core.Interfaces;

namespace GambaTracker.Api.Services;

public class ApiKeyService
{
    private readonly IApiKeyRepository _apiKeyRepository;

    public ApiKeyService(IApiKeyRepository apiKeyRepository)
    {
        _apiKeyRepository = apiKeyRepository;
    }

    public async Task<IEnumerable<ApiKeyResponse>> ListAsync(Guid userId)
    {
        var keys = await _apiKeyRepository.GetAllApiKeysByIdAsync(userId);
        return keys.Select(k => new ApiKeyResponse(k.Id, k.Name, k.Prefix, k.CreatedAt, k.LastUsedAt));
    }

    public async Task<CreatedApiKeyResponse> CreateAsync(Guid userId, CreateApiKeyRequest request)
    {
        var key = ApiKeyHasher.GenerateKey();
        var apiKey = new ApiKey
        {
            UserId = userId,
            Name = request.Name.Trim(),
            KeyHash = ApiKeyHasher.Hash(key),
            Prefix = key[..ApiKeyHasher.DisplayPrefixLength],
            CreatedAt = DateTime.UtcNow
        };

        await _apiKeyRepository.CreateApiKeyAsync(apiKey);
        return new CreatedApiKeyResponse(apiKey.Id, apiKey.Name, apiKey.Prefix, apiKey.CreatedAt, key);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        if (await _apiKeyRepository.GetApiKeyByIdAsync(id, userId) == null)
        {
            return false;
        }

        await _apiKeyRepository.DeleteApiKeyAsync(id, userId);
        return true;
    }
}
