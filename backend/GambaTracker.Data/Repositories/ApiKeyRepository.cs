using Dapper;
using GambaTracker.Core.Interfaces;

namespace GambaTracker.Data.Repositories
{
    public class ApiKeyRepository : IApiKeyRepository
    {
        private readonly DapperContext _configuration;
        public ApiKeyRepository(DapperContext configuration)
        {
            _configuration = configuration;
        }

        public async Task<ApiKey> CreateApiKeyAsync(ApiKey apiKey)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync(
                "INSERT INTO api_keys (Id, UserId, Name, KeyHash, Prefix, CreatedAt) VALUES (@Id, @UserId, @Name, @KeyHash, @Prefix, @CreatedAt)",
                apiKey);
            return apiKey;
        }

        public async Task DeleteApiKeyAsync(Guid id, Guid userId)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("DELETE FROM api_keys WHERE Id = @Id AND UserId = @UserId", new { Id = id, UserId = userId });
        }

        public async Task<IEnumerable<ApiKey>> GetAllApiKeysByIdAsync(Guid userId)
        {
            using var conn = _configuration.CreateConnection();
            return await conn.QueryAsync<ApiKey>("SELECT * FROM api_keys WHERE UserId = @UserId ORDER BY CreatedAt", new { UserId = userId });
        }

        public async Task<ApiKey?> GetApiKeyByHashAsync(string keyHash)
        {
            using var conn = _configuration.CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<ApiKey>("SELECT * FROM api_keys WHERE KeyHash = @KeyHash", new { KeyHash = keyHash });
        }

        public async Task<ApiKey?> GetApiKeyByIdAsync(Guid id, Guid userId)
        {
            using var conn = _configuration.CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<ApiKey>("SELECT * FROM api_keys WHERE Id = @Id AND UserId = @UserId", new { Id = id, UserId = userId });
        }

        public async Task UpdateLastUsedAsync(Guid id, DateTime lastUsedAt)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("UPDATE api_keys SET LastUsedAt = @LastUsedAt WHERE Id = @Id", new { Id = id, LastUsedAt = lastUsedAt });
        }
    }
}
