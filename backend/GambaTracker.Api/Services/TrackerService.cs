using GambaTracker.Core.DTOs;
using GambaTracker.Core.Interfaces;
using Npgsql;

namespace GambaTracker.Api.Services;

public class TrackerService
{
    private readonly ITrackerRepository _trackerRepository;
    private readonly IFolderRepository _folderRepository;

    public TrackerService(ITrackerRepository trackerRepository, IFolderRepository folderRepository)
    {
        _trackerRepository = trackerRepository;
        _folderRepository = folderRepository;
    }

    public async Task<TrackerResponse?> GetAsync(Guid id, Guid userId)
    {
        var tracker = await _trackerRepository.GetTrackerByIdAsync(id, userId);
        return tracker == null ? null : ToResponse(tracker);
    }

    public async Task<OperationResult<TrackerResponse>> CreateAsync(Guid userId, CreateTrackerRequest request)
    {
        if (await _folderRepository.GetFolderByIdAsync(request.FolderId, userId) == null)
        {
            return OperationResult<TrackerResponse>.NotFound();
        }

        var tracker = new Tracker
        {
            Name = request.Name.Trim(),
            FolderId = request.FolderId
        };

        try
        {
            await _trackerRepository.CreateTrackerAsync(tracker);
        }
        catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return OperationResult<TrackerResponse>.NameTaken();
        }

        return OperationResult<TrackerResponse>.Success(ToResponse(tracker));
    }

    public async Task<OperationResult<TrackerResponse>> RenameAsync(Guid id, Guid userId, RenameTrackerRequest request)
    {
        var tracker = await _trackerRepository.GetTrackerByIdAsync(id, userId);
        if (tracker == null)
        {
            return OperationResult<TrackerResponse>.NotFound();
        }

        tracker.Name = request.Name.Trim();

        try
        {
            await _trackerRepository.UpdateTrackerAsync(tracker);
        }
        catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return OperationResult<TrackerResponse>.NameTaken();
        }

        return OperationResult<TrackerResponse>.Success(ToResponse(tracker));
    }

    // Deletes the tracker and, through ON DELETE CASCADE, its events and drops
    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var tracker = await _trackerRepository.GetTrackerByIdAsync(id, userId);
        if (tracker == null)
        {
            return false;
        }

        await _trackerRepository.DeleteTrackerAsync(id, userId);
        return true;
    }

    private static TrackerResponse ToResponse(Tracker tracker) =>
        new(tracker.Id, tracker.Name, tracker.FolderId);
}
