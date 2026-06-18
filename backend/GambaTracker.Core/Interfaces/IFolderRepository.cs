namespace GambaTracker.Core.Interfaces
{
    public interface IFolderRepository
    {
        Task<Folder> GetFolderByIdAsync(Guid id);
        Task<IEnumerable<Folder>> GetAllFolderByIdAsync(Guid userId);
        Task<Folder> CreateFolderAsync(Folder folder);
        Task UpdateFolderAsync(Folder folder);
        Task DeleteFolderAsync(Guid id);
    }
}