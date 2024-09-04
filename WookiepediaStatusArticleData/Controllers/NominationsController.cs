using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Nominations;
using WookiepediaStatusArticleData.Services;
using WookiepediaStatusArticleData.Services.Nominations;

namespace WookiepediaStatusArticleData.Controllers;

[Authorize]
[Route("nominations")]
public class NominationsController(WookiepediaDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] NominationQuery query,
        [FromServices] NominationLookup lookup,
        CancellationToken cancellationToken
    )
    {
        var page = await lookup.LookupAsync(query, cancellationToken);
        return View(page);
    }

    [HttpGet("upload")]
    public IActionResult UploadForm()
    {
        return View();
    }
    
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        [FromForm] NominationImportForm form,
        [FromServices] NominationImporter importer,
        CancellationToken cancellationToken    
    )
    {
        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            return View("UploadForm", form);
        }
        
        try
        {
            await using var stream = form.Upload.OpenReadStream();
            await importer.ExecuteAsync(stream, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return RedirectToAction("Index");
        }
        catch (ValidationException ex)
        {
            foreach (var issue in ex.Issues)
            {
                ModelState.AddModelError("Upload", issue.Message);
            }
            
            Response.StatusCode = 400;
            return View("UploadForm", form);
        }
    }
}