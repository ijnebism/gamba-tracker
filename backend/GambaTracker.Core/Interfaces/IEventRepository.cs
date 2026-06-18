namespace GambaTracker.Core.Interfaces
{
    public interface IEventRepository
    {
        Task<Event> GetEventByIdAsync(Guid id);
        Task<IEnumerable<Event>> GetAllEventsByIdAsync(Guid trackerId);
        Task<Event> CreateEventAsync(Event Event);
        Task UpdateEventAsync(Event Event);
        Task DeleteEventAsync(Guid id);
    }
}