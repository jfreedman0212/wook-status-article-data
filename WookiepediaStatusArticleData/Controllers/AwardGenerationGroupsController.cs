using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Services.Awards;

namespace WookiepediaStatusArticleData.Controllers;

[Authorize]
[Route("award-generation-groups")]
public class AwardGenerationGroupsController(WookiepediaDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var groups = await db.Set<AwardGenerationGroup>()
            .OrderByDescending(g => g.StartedAt)
            .ThenByDescending(g => g.EndedAt)
            .ThenBy(g => g.Name)
            .ToListAsync(cancellationToken);

        return View(new AwardGenerationGroupsViewModel { Groups = groups });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        await using var txn = await db.Database.BeginTransactionAsync(cancellationToken);

        await db.Set<Award>()
            .Where(g => g.GenerationGroupId == id)
            .ExecuteDeleteAsync(cancellationToken);

        await db.Set<ProjectAward>()
            .Where(g => g.GenerationGroupId == id)
            .ExecuteDeleteAsync(cancellationToken);

        await db.Set<AwardGenerationGroup>()
            .Where(g => g.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        await txn.CommitAsync(cancellationToken);

        return Ok();
    }

    [HttpGet("new")]
    public IActionResult CreateForm()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromForm] AwardGenerationGroupForm form,
        [FromServices] GenerateAwardsAction generateAwardsAction,
        CancellationToken cancellationToken
    )
    {
        var startedAt = form.StartedAt.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endedAt = form.EndedAt.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        if (form.StartedAt >= form.EndedAt)
        {
            ModelState.AddModelError(nameof(form.StartedAt), "Started At must be before Ended At");
        }

        var nameAlreadyExists = await db.Set<AwardGenerationGroup>()
            .AnyAsync(
                c => c.Name == form.Name && c.StartedAt == startedAt && c.EndedAt == endedAt,
                cancellationToken
            );

        if (nameAlreadyExists)
        {
            ModelState.AddModelError(nameof(form.Name), $"{form.Name} already exists for that timeframe");
        }

        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            return View("CreateForm", form);
        }

        var now = DateTime.UtcNow;
        var newEntity = new AwardGenerationGroup
        {
            Name = form.Name,
            StartedAt = startedAt,
            EndedAt = endedAt,
            Awards = [],
            ProjectAwards = [],
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Add(newEntity);

        await using var txn = await db.Database.BeginTransactionAsync(cancellationToken);
        await generateAwardsAction.ExecuteAsync(newEntity, cancellationToken);
        await txn.CommitAsync(cancellationToken);

        return RedirectToAction("Index");
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> RefreshAwards(
        [FromRoute] int id,
        [FromServices] GenerateAwardsAction generateAwardsAction,
        CancellationToken cancellationToken
    )
    {
        var awardGenerationGroup = await db.Set<AwardGenerationGroup>()
            .SingleOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (awardGenerationGroup == null)
        {
            return NotFound();
        }

        await using var txn = await db.Database.BeginTransactionAsync(cancellationToken);
        await generateAwardsAction.RefreshAsync(awardGenerationGroup, cancellationToken);
        await txn.CommitAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("{id:int}/export-wookieepedia")]
    public async Task<IActionResult> ExportToWookieepediaWikitext([FromRoute] int id, CancellationToken cancellationToken)
    {
        var group = await db.Set<AwardGenerationGroup>().SingleOrDefaultAsync(g => g.Id == id, cancellationToken);

        if (group == null)
        {
            return NotFound();
        }

        var content = "hello world";
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var fileName = $"award-generation-group-{id}.txt";

        return File(bytes, "text/plain; charset=utf-8", fileName);
    }
}