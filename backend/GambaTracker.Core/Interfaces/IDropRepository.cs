namespace GambaTracker.Core.Interfaces
{
    public interface IDropRepository
    {
        Task<Drop> GetDropByIdAsync(Guid id);
        Task<IEnumerable<Drop>> GetAllDropsByIdAsync(Guid EventId);
        Task<Drop> CreateDropAsync(Drop drop);
        Task UpdateDropAsync(Drop drop);
        Task DeleteDropAsync(Guid id);
    }
}