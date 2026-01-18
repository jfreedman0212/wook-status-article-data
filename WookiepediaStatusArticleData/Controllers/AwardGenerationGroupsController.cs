using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement.Mvc;
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

    [FeatureGate("ExportAwardsToWookieepedia")]
    [HttpGet("{id:int}/export-wookieepedia")]
    public async Task<IActionResult> ExportToWookieepediaWikitext(
        [FromRoute] int id,
        [FromServices] AwardsAggregationService awardsAggregationService,
        CancellationToken cancellationToken
    )
    {
        var group = await db.Set<AwardGenerationGroup>().SingleOrDefaultAsync(g => g.Id == id, cancellationToken);

        if (group == null)
        {
            return NotFound();
        }

        var content = "";

        var result = await awardsAggregationService.RetrieveTablesAsync(
            group,
            cancellationToken
        );

        // use this link as reference for table formatting:
        // https://www.mediawiki.org/wiki/Help:Tables

        foreach (var heading in result.Selected!.AwardHeadings)
        {
            content += $"== {heading.Heading} ==\n\n";

            foreach (var subheading in heading.Subheadings)
            {
                // open table w/ some config
                content += "{|{{Prettytable|style=margin:auto}}";

                // column headers
                var headers = new List<string>();
                var uhh = new Dictionary<string, Func<AwardViewModel, string>>();

                if (subheading.Mode is TableMode.LongestStatusArticle)
                {
                    uhh.Add("Article", awardViewModel => awardViewModel.Type);
                }
                else
                {
                    uhh.Add("Award", awardViewModel => awardViewModel.Type);
                }

                if (subheading.Mode is not TableMode.MostDaysWithArticles and not TableMode.MVP and not TableMode.LongestStatusArticle)
                {
                    uhh.Add("Place", awardViewModel => awardViewModel.Type);
                }

                if (subheading.Mode == TableMode.WookieeProject)
                {
                    headers.Add("Project(s)");
                }
                else if (subheading.Mode == TableMode.MostDaysWithArticles)
                {
                    headers.Add("Date");
                }
                else
                {
                    headers.Add("Nominator(s)");
                }

                if (subheading.Mode is TableMode.LongestStatusArticle)
                {
                    headers.Add("Word Count");
                }
                else
                {
                    headers.Add("Count");
                }

                content += $"! {string.Join(" !! ", headers)}\n";

                // rows in the table
                var rows = new List<string>();
                foreach (var awardType in subheading.Awards)
                {
                    for (var i = 0; i < awardType.Winners.Count; i++)
                    {
                        if (i == 0)
                        {

                        }
                    }
                }

                // closing table
                content += "|}";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var fileName = $"award-generation-group-{id}.txt";

            return File(bytes, "text/plain; charset=utf-8", fileName);
        }
    }
}