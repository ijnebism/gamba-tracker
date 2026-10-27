using Dapper;
using GambaTracker.Core.Interfaces;

namespace GambaTracker.Data.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly DapperContext _configuration;
        public EventRepository(DapperContext configuration)
        {
            _configuration = configuration;
        }
        public async Task<Event> CreateEventAsync(Event Event)
        {
            using var conn = _configuration.CreateConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            await conn.ExecuteAsync("INSERT INTO Events (Id, Timestamp, TrackerId) VALUES (@Id, @Timestamp, @TrackerId)", Event, transaction);

            foreach (var drop in Event.Drops)
            {
                drop.EventId = Event.Id;
            }
            // Dapper runs the insert once per drop
            await conn.ExecuteAsync("INSERT INTO Drops (Id, ItemName, Quantity, EventId) VALUES (@Id, @ItemName, @Quantity, @EventId)", Event.Drops, transaction);

            transaction.Commit();
            return Event;
        }

        public async Task DeleteEventAsync(Guid id, Guid userId)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync(
                @"DELETE FROM Events e USING Trackers t, Folders f
                  WHERE e.Id = @Id AND e.TrackerId = t.Id AND t.FolderId = f.Id AND f.UserId = @UserId",
                new { Id = id, UserId = userId });
        }

        public async Task<IEnumerable<Event>> GetEventsWithDropsAsync(Guid trackerId, DateTime? from, DateTime? to)
        {
            // Filters are only added when given, so the SQL stays fully parameterised
            var filter = "e.TrackerId = @TrackerId";
            if (from.HasValue) filter += " AND e.Timestamp >= @From";
            if (to.HasValue) filter += " AND e.Timestamp <= @To";
            var parameters = new { TrackerId = trackerId, From = from, To = to };

            using var conn = _configuration.CreateConnection();
            var events = (await conn.QueryAsync<Event>(
                $"SELECT e.* FROM Events e WHERE {filter} ORDER BY e.Timestamp DESC", parameters)).ToList();

            var drops = await conn.QueryAsync<Drop>(
                $"SELECT d.* FROM Drops d JOIN Events e ON e.Id = d.EventId WHERE {filter} ORDER BY d.ItemName", parameters);

            var dropsByEvent = drops.ToLookup(d => d.EventId);
            foreach (var e in events)
            {
                e.Drops = dropsByEvent[e.Id].ToList();
            }
            return events;
        }

        public async Task<Event?> GetEventByIdAsync(Guid id, Guid userId)
        {
            using var conn = _configuration.CreateConnection();
            var Event = await conn.QuerySingleOrDefaultAsync<Event>(
                @"SELECT e.* FROM Events e
                  JOIN Trackers t ON t.Id = e.TrackerId
                  JOIN Folders f ON f.Id = t.FolderId
                  WHERE e.Id = @Id AND f.UserId = @UserId",
                new { Id = id, UserId = userId });
            return Event;
        }

        public async Task UpdateEventAsync(Event Event)
        {
            using var conn = _configuration.CreateConnection();
            await conn.ExecuteAsync("UPDATE Events SET Timestamp = @Timestamp WHERE Id = @Id", Event);
        }
    }
}
