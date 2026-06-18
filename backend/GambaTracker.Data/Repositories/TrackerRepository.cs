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

        public async Task DeleteTrackerAsync(Guid id)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("DELETE FROM Trackers WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Tracker>> GetAllTrackersByIdAsync(Guid folderId)
        {
            using var conn = _configuration.CreateConnection();
            var Trackers = await conn.QueryAsync<Tracker>("SELECT * FROM Trackers WHERE FolderId = @FolderId", new { FolderId = folderId });
            return Trackers;
        }

        public async Task<Tracker> GetTrackerByIdAsync(Guid id)
        {
            using var conn = _configuration.CreateConnection();
            var Tracker = await conn.QuerySingleAsync<Tracker>("SELECT * FROM Trackers WHERE Id = @Id", new { Id = id });
            return Tracker;
        }

        public async Task UpdateTrackerAsync(Tracker Tracker)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("UPDATE Trackers SET Name = @Name, FolderId = @FolderId WHERE Id = @Id", Tracker);
        }
    }
}