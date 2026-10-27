namespace GambaTracker.Core.Interfaces
{
    public interface ITrackerRepository
    {
        // Returns null unless the tracker's folder belongs to the user
        Task<Tracker?> GetTrackerByIdAsync(Guid id, Guid userId);
        // Callers must check the folder belongs to the user first
        Task<IEnumerable<Tracker>> GetAllTrackersByIdAsync(Guid folderId);
        Task<Tracker> CreateTrackerAsync(Tracker Tracker);
        // Callers must check the tracker belongs to the user first
        Task UpdateTrackerAsync(Tracker Tracker);
        Task DeleteTrackerAsync(Guid id, Guid userId);
    }
}
