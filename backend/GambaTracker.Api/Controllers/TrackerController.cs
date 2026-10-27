using GambaTracker.Api.Extensions;
using GambaTracker.Api.Services;
using GambaTracker.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GambaTracker.Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize]
  public class TrackerController : ControllerBase
  {
    private readonly TrackerService _trackerService;

    public TrackerController(TrackerService trackerService)
    {
      _trackerService = trackerService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrackerResponse>> GetTracker(Guid id)
    {
      var tracker = await _trackerService.GetAsync(id, User.GetUserId());
      if (tracker == null)
      {
        return NotFound();
      }
      return Ok(tracker);
    }

    [HttpPost]
    public async Task<ActionResult<TrackerResponse>> CreateTracker(CreateTrackerRequest request)
    {
      var result = await _trackerService.CreateAsync(User.GetUserId(), request);
      return result.Status switch
      {
        OperationStatus.NotFound => NotFound("Folder not found."),
        OperationStatus.NameTaken => Conflict("A tracker with that name already exists in this folder."),
        _ => CreatedAtAction(nameof(GetTracker), new { id = result.Value!.Id }, result.Value)
      };
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TrackerResponse>> RenameTracker(Guid id, RenameTrackerRequest request)
    {
      var result = await _trackerService.RenameAsync(id, User.GetUserId(), request);
      return result.Status switch
      {
        OperationStatus.NotFound => NotFound(),
        OperationStatus.NameTaken => Conflict("A tracker with that name already exists in this folder."),
        _ => Ok(result.Value)
      };
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTracker(Guid id)
    {
      var deleted = await _trackerService.DeleteAsync(id, User.GetUserId());
      return deleted ? NoContent() : NotFound();
    }
  }
}
