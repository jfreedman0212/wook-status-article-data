using Htmx;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Nominations.Projects;
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
        var allProjects = await db.Set<Project>()
            .Where(it => !it.IsArchived)
            .OrderBy(it => it.Name)
            .ToListAsync(cancellationToken);
        var allNominators = await db.Set<Nominator>()
            .OrderBy(it => it.Name)
            .ToListAsync(cancellationToken);

        ViewBag.AllProjects = allProjects
            .Select(it => new SelectListItem(it.Name, it.Id.ToString()))
            .ToList();

        ViewBag.AllNominators = allNominators
            .Select(it => new SelectListItem(it.Name, it.Id.ToString()))
            .ToList();

        var page = await lookup.LookupAsync(query, cancellationToken);

        return Request.IsHtmx()
            // htmx requests just need the table rows
            ? PartialView("_TableRows", page)
            // whereas normal requests need to render the whole page
            : View(page);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> EditForm(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var nomination = await db.Set<Nomination>()
            .Include(it => it.Nominators)
            .Include(it => it.Projects)
            .SingleOrDefaultAsync(it => it.Id == id, cancellationToken);

        if (nomination == null) return NotFound();

        var allProjects = await db.Set<Project>()
            .Where(it => !it.IsArchived)
            .OrderBy(it => it.Name)
            .ToListAsync(cancellationToken);
        var allNominators = await db.Set<Nominator>()
            .OrderBy(it => it.Name)
            .ToListAsync(cancellationToken);

        ViewBag.AllProjects = allProjects
            .Select(it => new SelectListItem(it.Name, it.Id.ToString()))
            .ToList();

        ViewBag.AllNominators = allNominators
            .Select(it => new SelectListItem(it.Name, it.Id.ToString()))
            .ToList();

        return View(new NominationForm
        {
            Id = nomination.Id,
            ArticleName = nomination.ArticleName,
            Continuities = nomination.Continuities,
            Type = nomination.Type,
            Outcome = nomination.Outcome,
            StartedAtDate = DateOnly.FromDateTime(nomination.StartedAt),
            StartedAtTime = TimeOnly.FromDateTime(nomination.StartedAt),
            EndedAtDate = nomination.EndedAt != null ? DateOnly.FromDateTime(nomination.EndedAt.Value) : null,
            EndedAtTime = nomination.EndedAt != null ? TimeOnly.FromDateTime(nomination.EndedAt.Value) : null,
            StartWordCount = nomination.StartWordCount ?? 0,
            EndWordCount = nomination.EndWordCount ?? 0,
            NominatorIds = nomination.Nominators!.Select(it => it.Id).ToList(),
            ProjectIds = nomination.Projects!.Select(it => it.Id).ToList()
        });
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