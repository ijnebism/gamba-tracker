using System.ComponentModel.DataAnnotations;

namespace GambaTracker.Core.DTOs
{
    // e.g. { "path": "Skyblock/Dungeons/F7", "tracker": "Chest drops", "drops": [{ "itemName": "Necron's Handle", "quantity": 1 }] }
    // Path is the folder path from the user's root, separated by "/". Missing folders and the tracker are created.
    // Timestamp defaults to the time the request is received.
    public record IngestRequest(
        [Required, StringLength(1000)] string Path,
        [Required, StringLength(100)] string Tracker,
        DateTimeOffset? Timestamp,
        [Required, MinLength(1)] List<DropDto> Drops);

    public record IngestResponse(Guid FolderId, Guid TrackerId, EventResponse Event);
}
