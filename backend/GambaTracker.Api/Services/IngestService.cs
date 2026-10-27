using GambaTracker.Core.DTOs;
using GambaTracker.Core.Interfaces;

namespace GambaTracker.Api.Services;

public class IngestService
{
    public const int MaxFolderDepth = 20;
    public const int MaxNameLength = 100;

    private readonly IIngestRepository _ingestRepository;

    public IngestService(IIngestRepository ingestRepository)
    {
        _ingestRepository = ingestRepository;
    }

    // Splits "Skyblock / Dungeons/F7" into ["Skyblock", "Dungeons", "F7"], ignoring empty segments.
    // Returns an error message instead when the path can't be used.
    public static (string[] Segments, string? Error) ParsePath(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (segments.Length == 0)
            return (segments, "Path must contain at least one folder name.");
        if (segments.Length > MaxFolderDepth)
            return (segments, $"Path can be at most {MaxFolderDepth} folders deep.");
        if (segments.Any(s => s.Length > MaxNameLength))
            return (segments, $"Folder names can be at most {MaxNameLength} characters.");

        return (segments, null);
    }

    public async Task<IngestResponse> IngestAsync(Guid userId, string[] folderPath, IngestRequest request)
    {
        var eventItem = new Event
        {
            Timestamp = (request.Timestamp ?? DateTimeOffset.UtcNow).UtcDateTime,
            Drops = request.Drops
                .Select(d => new Drop { ItemName = d.ItemName.Trim(), Quantity = d.Quantity })
                .ToList()
        };

        var (folderId, trackerId) = await _ingestRepository.IngestEventAsync(userId, folderPath, request.Tracker.Trim(), eventItem);
        return new IngestResponse(folderId, trackerId, EventService.ToResponse(eventItem));
    }
}
