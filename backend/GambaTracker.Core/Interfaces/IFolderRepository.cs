namespace GambaTracker.Core.Interfaces
{
    // Every method is scoped to a user, so callers can't read or change another user's folders
    public interface IFolderRepository
    {
        Task<Folder?> GetFolderByIdAsync(Guid id, Guid userId);
        Task<IEnumerable<Folder>> GetAllFolderByIdAsync(Guid userId);
        // parentFolderId null returns the folders at the user's root
        Task<IEnumerable<Folder>> GetChildFoldersAsync(Guid userId, Guid? parentFolderId);
        Task<Folder> CreateFolderAsync(Folder folder);
        Task UpdateFolderAsync(Folder folder);
        Task DeleteFolderAsync(Guid id, Guid userId);
    }
}
