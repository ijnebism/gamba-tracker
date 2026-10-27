namespace GambaTracker.Core.Interfaces
{
    public interface IApiKeyRepository
    {
        Task<ApiKey?> GetApiKeyByIdAsync(Guid id, Guid userId);
        // Used by the authentication handler to resolve an incoming key
        Task<ApiKey?> GetApiKeyByHashAsync(string keyHash);
        Task<IEnumerable<ApiKey>> GetAllApiKeysByIdAsync(Guid userId);
        Task<ApiKey> CreateApiKeyAsync(ApiKey apiKey);
        Task UpdateLastUsedAsync(Guid id, DateTime lastUsedAt);
        Task DeleteApiKeyAsync(Guid id, Guid userId);
    }
}
