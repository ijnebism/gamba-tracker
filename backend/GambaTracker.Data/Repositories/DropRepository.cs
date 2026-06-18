using Dapper;
using GambaTracker.Core.Interfaces;

namespace GambaTracker.Data.Repositories
{
    public class DropRepository : IDropRepository
    {
        private readonly DapperContext _configuration;
        public DropRepository(DapperContext configuration)
        {
            _configuration = configuration;
        }
        public async Task<Drop> CreateDropAsync(Drop drop)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("INSERT INTO Drops (Id, Name, EventId) VALUES (@Id, @Name, @EventId)", drop);
            return drop;
        }

        public async Task DeleteDropAsync(Guid id)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("DELETE FROM Drops WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Drop>> GetAllDropsByIdAsync(Guid EventId)
        {
            using var conn = _configuration.CreateConnection();
            var drops = await conn.QueryAsync<Drop>("SELECT * FROM Drops WHERE EventId = @EventId", new { EventId = EventId });
            return drops;
        }

        public async Task<Drop> GetDropByIdAsync(Guid id)
        {
            using var conn = _configuration.CreateConnection();
            var drop = await conn.QuerySingleAsync<Drop>("SELECT * FROM Drops WHERE Id = @Id", new { Id = id });
            return drop;
        }

        public async Task UpdateDropAsync(Drop drop)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("UPDATE Drops SET Name = @Name, EventId = @EventId WHERE Id = @Id", drop);
        }
        
    }
}