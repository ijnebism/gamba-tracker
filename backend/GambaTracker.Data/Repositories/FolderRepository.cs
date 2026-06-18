using Dapper;
using GambaTracker.Core.Interfaces;

namespace GambaTracker.Data.Repositories
{
    public class FolderRepository : IFolderRepository
    {
        private readonly DapperContext _configuration;
        public FolderRepository(DapperContext configuration)
        {
            _configuration = configuration;
        }
        public async Task<Folder> CreateFolderAsync(Folder folder)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("INSERT INTO Folders (Id, Name, UserId) VALUES (@Id, @Name, @UserId)", folder);
            return folder;
        }

        public async Task DeleteFolderAsync(Guid id)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("DELETE FROM Folders WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Folder>> GetAllFolderByIdAsync(Guid userId)
        {
            using var conn = _configuration.CreateConnection();
            var folders = await conn.QueryAsync<Folder>("SELECT * FROM Folders WHERE UserId = @UserId", new { UserId = userId });
            return folders;
        }

        public async Task<Folder> GetFolderByIdAsync(Guid id)
        {
            using var conn = _configuration.CreateConnection();
            var folder = await conn.QuerySingleAsync<Folder>("SELECT * FROM Folders WHERE Id = @Id", new { Id = id });
            return folder;
        }

        public async Task UpdateFolderAsync(Folder folder)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("UPDATE Folders SET Name = @Name WHERE Id = @Id", folder);
        }
    }
}