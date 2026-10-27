using System.Data;
using Dapper;

namespace GambaTracker.Data;

// Shared by EventRepository and IngestRepository so both insert events the same way inside their own transaction
internal static class EventWriter
{
    public static async Task InsertWithDropsAsync(IDbConnection conn, IDbTransaction transaction, Event eventItem)
    {
        await conn.ExecuteAsync("INSERT INTO Events (Id, Timestamp, TrackerId) VALUES (@Id, @Timestamp, @TrackerId)", eventItem, transaction);

        foreach (var drop in eventItem.Drops)
        {
            drop.EventId = eventItem.Id;
        }
        // Dapper runs the insert once per drop
        await conn.ExecuteAsync("INSERT INTO Drops (Id, ItemName, Quantity, EventId) VALUES (@Id, @ItemName, @Quantity, @EventId)", eventItem.Drops, transaction);
    }
}
