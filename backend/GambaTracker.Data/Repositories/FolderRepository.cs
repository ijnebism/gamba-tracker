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
            await conn.ExecuteAsync("INSERT INTO Folders (Id, Name, UserId, ParentFolderId) VALUES (@Id, @Name, @UserId, @ParentFolderId)", folder);
            return folder;
        }

        public async Task DeleteFolderAsync(Guid id, Guid userId)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("DELETE FROM Folders WHERE Id = @Id AND UserId = @UserId", new { Id = id, UserId = userId });
        }

        public async Task<IEnumerable<Folder>> GetAllFolderByIdAsync(Guid userId)
        {
            using var conn = _configuration.CreateConnection();
            var folders = await conn.QueryAsync<Folder>("SELECT * FROM Folders WHERE UserId = @UserId", new { UserId = userId });
            return folders;
        }

        public async Task<IEnumerable<Folder>> GetChildFoldersAsync(Guid userId, Guid? parentFolderId)
        {
            using var conn = _configuration.CreateConnection();
            // IS NOT DISTINCT FROM treats NULL = NULL as true, so a null parent matches root folders
            var folders = await conn.QueryAsync<Folder>(
                "SELECT * FROM Folders WHERE UserId = @UserId AND ParentFolderId IS NOT DISTINCT FROM @ParentFolderId ORDER BY Name",
                new { UserId = userId, ParentFolderId = parentFolderId });
            return folders;
        }

        public async Task<Folder?> GetFolderByIdAsync(Guid id, Guid userId)
        {
            using var conn = _configuration.CreateConnection();
            var folder = await conn.QuerySingleOrDefaultAsync<Folder>("SELECT * FROM Folders WHERE Id = @Id AND UserId = @UserId", new { Id = id, UserId = userId });
            return folder;
        }

        public async Task UpdateFolderAsync(Folder folder)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("UPDATE Folders SET Name = @Name, ParentFolderId = @ParentFolderId WHERE Id = @Id AND UserId = @UserId", folder);
        }
    }
}
