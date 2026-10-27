using GambaTracker.Api.Authentication;
using GambaTracker.Api.Extensions;
using GambaTracker.Api.Services;
using GambaTracker.Core.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GambaTracker.Api.Controllers
{
  // External sources send an X-Api-Key header. A JWT also works, so ingest can be tried from Swagger or the app
  [Route("api/[controller]")]
  [ApiController]
  [Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName + "," + JwtBearerDefaults.AuthenticationScheme)]
  public class IngestController : ControllerBase
  {
    private readonly IngestService _ingestService;

    public IngestController(IngestService ingestService)
    {
      _ingestService = ingestService;
    }

    [HttpPost]
    public async Task<ActionResult<IngestResponse>> Ingest(IngestRequest request)
    {
      var (folderPath, error) = IngestService.ParsePath(request.Path);
      if (error != null)
      {
        ModelState.AddModelError(nameof(request.Path), error);
        return ValidationProblem(ModelState);
      }

      var result = await _ingestService.IngestAsync(User.GetUserId(), folderPath, request);
      return CreatedAtAction(nameof(EventController.GetEvent), "Event", new { id = result.Event.Id }, result);
    }
  }
}
