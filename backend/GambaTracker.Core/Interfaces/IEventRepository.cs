namespace GambaTracker.Core.Interfaces
{
    public interface IEventRepository
    {
        // Returns null unless the event's tracker is in one of the user's folders
        Task<Event?> GetEventByIdAsync(Guid id, Guid userId);
        // Newest first, each with its Drops loaded. Callers must check the tracker belongs to the user first
        Task<IEnumerable<Event>> GetEventsWithDropsAsync(Guid trackerId, DateTime? from, DateTime? to);
        // Inserts the event and all of its Drops in one transaction
        Task<Event> CreateEventAsync(Event Event);
        // Callers must check the event belongs to the user first
        Task UpdateEventAsync(Event Event);
        Task DeleteEventAsync(Guid id, Guid userId);
    }
}
