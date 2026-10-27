using GambaTracker.Api.Extensions;
using GambaTracker.Api.Services;
using GambaTracker.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GambaTracker.Api.Controllers
{
  // Events are created and listed under their tracker, but fetched and deleted by their own id
  [Route("api")]
  [ApiController]
  [Authorize]
  public class EventController : ControllerBase
  {
    private readonly EventService _eventService;

    public EventController(EventService eventService)
    {
      _eventService = eventService;
    }

    [HttpGet("tracker/{trackerId:guid}/events")]
    public async Task<ActionResult<IEnumerable<EventResponse>>> ListEvents(Guid trackerId, DateTimeOffset? from, DateTimeOffset? to)
    {
      var events = await _eventService.ListAsync(trackerId, User.GetUserId(), from, to);
      if (events == null)
      {
        return NotFound();
      }
      return Ok(events);
    }

    [HttpPost("tracker/{trackerId:guid}/events")]
    public async Task<ActionResult<EventResponse>> CreateEvent(Guid trackerId, CreateEventRequest request)
    {
      var result = await _eventService.CreateAsync(trackerId, User.GetUserId(), request);
      return result.Status switch
      {
        OperationStatus.NotFound => NotFound("Tracker not found."),
        _ => CreatedAtAction(nameof(GetEvent), new { id = result.Value!.Id }, result.Value)
      };
    }

    [HttpGet("event/{id:guid}")]
    public async Task<ActionResult<EventResponse>> GetEvent(Guid id)
    {
      var eventItem = await _eventService.GetAsync(id, User.GetUserId());
      if (eventItem == null)
      {
        return NotFound();
      }
      return Ok(eventItem);
    }

    [HttpDelete("event/{id:guid}")]
    public async Task<IActionResult> DeleteEvent(Guid id)
    {
      var deleted = await _eventService.DeleteAsync(id, User.GetUserId());
      return deleted ? NoContent() : NotFound();
    }
  }
}
