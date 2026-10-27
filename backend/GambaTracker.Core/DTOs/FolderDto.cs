using System.ComponentModel.DataAnnotations;

namespace GambaTracker.Core.DTOs
{
    // ParentFolderId null creates the folder at the user's root
    public record CreateFolderRequest([Required, StringLength(100)] string Name, Guid? ParentFolderId);
    public record RenameFolderRequest([Required, StringLength(100)] string Name);

    public record FolderResponse(Guid Id, string Name, Guid? ParentFolderId);

    // One Browse screen: the folder itself (null at root) plus its direct children
    public record FolderContentsResponse(FolderResponse? Folder, IEnumerable<FolderResponse> Folders, IEnumerable<TrackerResponse> Trackers);
}
