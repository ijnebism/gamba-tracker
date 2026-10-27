using System.ComponentModel.DataAnnotations;

namespace GambaTracker.Core.DTOs
{
    public record CreateTrackerRequest([Required, StringLength(100)] string Name, Guid FolderId);
    public record RenameTrackerRequest([Required, StringLength(100)] string Name);

    public record TrackerResponse(Guid Id, string Name, Guid FolderId);
}
