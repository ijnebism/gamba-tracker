namespace GambaTracker.Core.Interfaces
{
    public interface ITrackerRepository
    {
        Task<Tracker> GetTrackerByIdAsync(Guid id);
        Task<IEnumerable<Tracker>> GetAllTrackersByIdAsync(Guid folderId);
        Task<Tracker> CreateTrackerAsync(Tracker Tracker);
        Task UpdateTrackerAsync(Tracker Tracker);
        Task DeleteTrackerAsync(Guid id);
    }
}