using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Nominations;
using WookiepediaStatusArticleData.Services;
using WookiepediaStatusArticleData.Services.Nominations;

namespace WookiepediaStatusArticleData.Controllers;

[Authorize]
[ApiController]
[Route("nominations")]
public class NominationsController(WookiepediaDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Paginate(
        [FromQuery] NominationQuery query,
        [FromServices] NominationLookup lookup,
        CancellationToken cancellationToken
    )
    {
        return Ok(await lookup.LookupAsync(query, cancellationToken));
    }
    
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        [FromServices] NominationImporter importer,
        CancellationToken cancellationToken    
    )
    {
        try
        {
            await importer.ExecuteAsync(Request.Body, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            foreach (var issue in ex.Issues)
            {
                ModelState.AddModelError("Upload", issue.Message);
            }
            
            return ValidationProblem(ModelState);
        }
    }
}