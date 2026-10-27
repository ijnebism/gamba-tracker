using Dapper;
using GambaTracker.Core.Interfaces;

namespace GambaTracker.Data.Repositories
{
    public class TrackerRepository : ITrackerRepository
    {
        private readonly DapperContext _configuration;
        public TrackerRepository(DapperContext configuration)
        {
            _configuration = configuration;
        }
        public async Task<Tracker> CreateTrackerAsync(Tracker Tracker)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("INSERT INTO Trackers (Id, Name, FolderId) VALUES (@Id, @Name, @FolderId)", Tracker);
            return Tracker;
        }

        public async Task DeleteTrackerAsync(Guid id, Guid userId)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync(
                "DELETE FROM Trackers t USING Folders f WHERE t.Id = @Id AND t.FolderId = f.Id AND f.UserId = @UserId",
                new { Id = id, UserId = userId });
        }

        public async Task<IEnumerable<Tracker>> GetAllTrackersByIdAsync(Guid folderId)
        {
            using var conn = _configuration.CreateConnection();
            var Trackers = await conn.QueryAsync<Tracker>("SELECT * FROM Trackers WHERE FolderId = @FolderId", new { FolderId = folderId });
            return Trackers;
        }

        public async Task<Tracker?> GetTrackerByIdAsync(Guid id, Guid userId)
        {
            using var conn = _configuration.CreateConnection();
            var Tracker = await conn.QuerySingleOrDefaultAsync<Tracker>(
                "SELECT t.* FROM Trackers t JOIN Folders f ON f.Id = t.FolderId WHERE t.Id = @Id AND f.UserId = @UserId",
                new { Id = id, UserId = userId });
            return Tracker;
        }

        public async Task UpdateTrackerAsync(Tracker Tracker)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("UPDATE Trackers SET Name = @Name, FolderId = @FolderId WHERE Id = @Id", Tracker);
        }
    }
}