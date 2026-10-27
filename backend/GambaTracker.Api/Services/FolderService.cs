using GambaTracker.Core.DTOs;
using GambaTracker.Core.Interfaces;
using Npgsql;

namespace GambaTracker.Api.Services;

public class FolderService
{
    private readonly IFolderRepository _folderRepository;
    private readonly ITrackerRepository _trackerRepository;

    public FolderService(IFolderRepository folderRepository, ITrackerRepository trackerRepository)
    {
        _folderRepository = folderRepository;
        _trackerRepository = trackerRepository;
    }

    public async Task<FolderContentsResponse> GetRootAsync(Guid userId)
    {
        var folders = await _folderRepository.GetChildFoldersAsync(userId, null);
        // Trackers always belong to a folder, so the root only holds folders
        return new FolderContentsResponse(null, folders.Select(ToResponse), []);
    }

    public async Task<FolderContentsResponse?> GetContentsAsync(Guid id, Guid userId)
    {
        var folder = await _folderRepository.GetFolderByIdAsync(id, userId);
        if (folder == null)
        {
            return null;
        }

        var folders = await _folderRepository.GetChildFoldersAsync(userId, id);
        var trackers = await _trackerRepository.GetAllTrackersByIdAsync(id);

        return new FolderContentsResponse(
            ToResponse(folder),
            folders.Select(ToResponse),
            trackers.OrderBy(t => t.Name).Select(t => new TrackerResponse(t.Id, t.Name, t.FolderId)));
    }

    public async Task<OperationResult<FolderResponse>> CreateAsync(Guid userId, CreateFolderRequest request)
    {
        if (request.ParentFolderId is Guid parentId && await _folderRepository.GetFolderByIdAsync(parentId, userId) == null)
        {
            return OperationResult<FolderResponse>.NotFound();
        }

        var folder = new Folder
        {
            Name = request.Name.Trim(),
            UserId = userId,
            ParentFolderId = request.ParentFolderId
        };

        try
        {
            await _folderRepository.CreateFolderAsync(folder);
        }
        catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return OperationResult<FolderResponse>.NameTaken();
        }

        return OperationResult<FolderResponse>.Success(ToResponse(folder));
    }

    public async Task<OperationResult<FolderResponse>> RenameAsync(Guid id, Guid userId, RenameFolderRequest request)
    {
        var folder = await _folderRepository.GetFolderByIdAsync(id, userId);
        if (folder == null)
        {
            return OperationResult<FolderResponse>.NotFound();
        }

        folder.Name = request.Name.Trim();

        try
        {
            await _folderRepository.UpdateFolderAsync(folder);
        }
        catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return OperationResult<FolderResponse>.NameTaken();
        }

        return OperationResult<FolderResponse>.Success(ToResponse(folder));
    }

    // Deletes the folder and, through ON DELETE CASCADE, everything inside it
    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var folder = await _folderRepository.GetFolderByIdAsync(id, userId);
        if (folder == null)
        {
            return false;
        }

        await _folderRepository.DeleteFolderAsync(id, userId);
        return true;
    }

    private static FolderResponse ToResponse(Folder folder) =>
        new(folder.Id, folder.Name, folder.ParentFolderId);
}
