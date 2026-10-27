using GambaTracker.Core.DTOs;
using GambaTracker.Core.Interfaces;

namespace GambaTracker.Api.Services;

public class EventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IDropRepository _dropRepository;
    private readonly ITrackerRepository _trackerRepository;

    public EventService(IEventRepository eventRepository, IDropRepository dropRepository, ITrackerRepository trackerRepository)
    {
        _eventRepository = eventRepository;
        _dropRepository = dropRepository;
        _trackerRepository = trackerRepository;
    }

    // Returns null when the tracker doesn't exist or isn't the user's
    public async Task<IEnumerable<EventResponse>?> ListAsync(Guid trackerId, Guid userId, DateTimeOffset? from, DateTimeOffset? to)
    {
        if (await _trackerRepository.GetTrackerByIdAsync(trackerId, userId) == null)
        {
            return null;
        }

        var events = await _eventRepository.GetEventsWithDropsAsync(trackerId, from?.UtcDateTime, to?.UtcDateTime);
        return events.Select(ToResponse);
    }

    public async Task<EventResponse?> GetAsync(Guid id, Guid userId)
    {
        var eventItem = await _eventRepository.GetEventByIdAsync(id, userId);
        if (eventItem == null)
        {
            return null;
        }

        eventItem.Drops = (await _dropRepository.GetAllDropsByIdAsync(id)).ToList();
        return ToResponse(eventItem);
    }

    public async Task<OperationResult<EventResponse>> CreateAsync(Guid trackerId, Guid userId, CreateEventRequest request)
    {
        if (await _trackerRepository.GetTrackerByIdAsync(trackerId, userId) == null)
        {
            return OperationResult<EventResponse>.NotFound();
        }

        var eventItem = new Event
        {
            Timestamp = request.Timestamp.UtcDateTime,
            TrackerId = trackerId,
            Drops = request.Drops
                .Select(d => new Drop { ItemName = d.ItemName.Trim(), Quantity = d.Quantity })
                .ToList()
        };

        await _eventRepository.CreateEventAsync(eventItem);
        return OperationResult<EventResponse>.Success(ToResponse(eventItem));
    }

    // Deletes the event and, through ON DELETE CASCADE, its drops
    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var eventItem = await _eventRepository.GetEventByIdAsync(id, userId);
        if (eventItem == null)
        {
            return false;
        }

        await _eventRepository.DeleteEventAsync(id, userId);
        return true;
    }

    // Also used by IngestService, so both return events in the same shape
    public static EventResponse ToResponse(Event eventItem) =>
        new(eventItem.Id, eventItem.Timestamp, eventItem.Drops.OrderBy(d => d.ItemName).Select(d => new DropDto(d.ItemName, d.Quantity)));
}
