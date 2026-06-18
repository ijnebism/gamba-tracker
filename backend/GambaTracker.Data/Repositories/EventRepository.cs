using Dapper;
using GambaTracker.Core.Interfaces;

namespace GambaTracker.Data.Repositories
{
    public class EventRepository : IEventRepository
    {
        public readonly DapperContext _configuration;
        public EventRepository(DapperContext configuration)
        {
            _configuration = configuration;
        }
        public async Task<Event> CreateEventAsync(Event Event)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("INSERT INTO Events (Id, Name, Date, TrackerId) VALUES (@Id, @Name, @Date, @TrackerId)", Event);
            return Event;
        }

        public async Task DeleteEventAsync(Guid id)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("DELETE FROM Events WHERE Id = @Id", new {Id = id});
        }

        public async Task<IEnumerable<Event>> GetAllEventsByIdAsync(Guid trackerId)
        {
            using var conn = _configuration.CreateConnection();
            var Events = await conn.QueryAsync<Event>("SELECT * FROM Events WHERE TrackerId = @TrackerId", new {TrackerId = trackerId});
            return Events;
        }

        public async Task<Event> GetEventByIdAsync(Guid id)
        {
            using var conn = _configuration.CreateConnection();
            var Event = await conn.QuerySingleAsync<Event>("SELECT * FROM Events WHERE Id = @Id", new {Id = id});
            return Event;
        }

        public async Task UpdateEventAsync(Event Event)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("UPDATE Events SET Name = @Name, Date = @Date WHERE Id = @Id", Event);
        }
    }
}