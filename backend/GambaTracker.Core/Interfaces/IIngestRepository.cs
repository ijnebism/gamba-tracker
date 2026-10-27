namespace GambaTracker.Core.Interfaces
{
    public interface IIngestRepository
    {
        // In one transaction: finds or creates each folder in folderPath (from the user's root down),
        // finds or creates the tracker in the last folder, then inserts the event and its Drops.
        // Sets eventItem.TrackerId and returns the ids of the last folder and the tracker.
        Task<(Guid FolderId, Guid TrackerId)> IngestEventAsync(Guid userId, IReadOnlyList<string> folderPath, string trackerName, Event eventItem);
    }
}
