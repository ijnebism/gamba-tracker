using GambaTracker.Api.Extensions;
using GambaTracker.Api.Services;
using GambaTracker.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GambaTracker.Api.Controllers
{
  // Plain [Authorize] uses the default (JWT) scheme only, so an API key can't be used to create or revoke keys
  [Route("api/[controller]")]
  [ApiController]
  [Authorize]
  public class ApiKeyController : ControllerBase
  {
    private readonly ApiKeyService _apiKeyService;

    public ApiKeyController(ApiKeyService apiKeyService)
    {
      _apiKeyService = apiKeyService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApiKeyResponse>>> ListApiKeys()
    {
      return Ok(await _apiKeyService.ListAsync(User.GetUserId()));
    }

    [HttpPost]
    public async Task<ActionResult<CreatedApiKeyResponse>> CreateApiKey(CreateApiKeyRequest request)
    {
      var created = await _apiKeyService.CreateAsync(User.GetUserId(), request);
      return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteApiKey(Guid id)
    {
      var deleted = await _apiKeyService.DeleteAsync(id, User.GetUserId());
      return deleted ? NoContent() : NotFound();
    }
  }
}
