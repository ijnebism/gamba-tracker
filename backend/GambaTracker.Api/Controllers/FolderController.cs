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
  public class FolderController : ControllerBase
  {
    private readonly FolderService _folderService;

    public FolderController(FolderService folderService)
    {
      _folderService = folderService;
    }

    [HttpGet("root")]
    public async Task<ActionResult<FolderContentsResponse>> GetRoot()
    {
      return Ok(await _folderService.GetRootAsync(User.GetUserId()));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FolderContentsResponse>> GetFolder(Guid id)
    {
      var contents = await _folderService.GetContentsAsync(id, User.GetUserId());
      if (contents == null)
      {
        return NotFound();
      }
      return Ok(contents);
    }

    [HttpPost]
    public async Task<ActionResult<FolderResponse>> CreateFolder(CreateFolderRequest request)
    {
      var result = await _folderService.CreateAsync(User.GetUserId(), request);
      return result.Status switch
      {
        OperationStatus.NotFound => NotFound("Parent folder not found."),
        OperationStatus.NameTaken => Conflict("A folder with that name already exists here."),
        _ => CreatedAtAction(nameof(GetFolder), new { id = result.Value!.Id }, result.Value)
      };
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FolderResponse>> RenameFolder(Guid id, RenameFolderRequest request)
    {
      var result = await _folderService.RenameAsync(id, User.GetUserId(), request);
      return result.Status switch
      {
        OperationStatus.NotFound => NotFound(),
        OperationStatus.NameTaken => Conflict("A folder with that name already exists here."),
        _ => Ok(result.Value)
      };
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFolder(Guid id)
    {
      var deleted = await _folderService.DeleteAsync(id, User.GetUserId());
      return deleted ? NoContent() : NotFound();
    }
  }
}
