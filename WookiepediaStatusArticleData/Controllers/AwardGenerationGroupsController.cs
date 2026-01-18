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

        var result = await awardsAggregationService.RetrieveTablesAsync(
            group,
            cancellationToken
        );

        if (result.Selected == null)
        {
            return NotFound();
        }

        var content = "";

        // use this link as reference for table formatting:
        // https://www.mediawiki.org/wiki/Help:Tables

        // Generate main award tables
        foreach (var heading in result.Selected.AwardHeadings)
        {
            content += $"== {heading.Heading} ==\n\n";

            foreach (var subheading in heading.Subheadings)
            {
                // Open table with config
                content += "{|{{Prettytable|style=margin:auto}}\n";

                // Table caption
                content += $"|+ {heading.Heading} ({subheading.Subheading})\n";

                // Column headers
                var headers = new List<string>();

                if (subheading.Mode is TableMode.LongestStatusArticle)
                {
                    headers.Add("Article");
                }
                else
                {
                    headers.Add("Award");
                }

                if (subheading.Mode is not TableMode.MostDaysWithArticles and not TableMode.MVP and not TableMode.LongestStatusArticle)
                {
                    headers.Add("Place");
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

                // Rows in the table
                foreach (var awardType in subheading.Awards)
                {
                    for (var i = 0; i < awardType.Winners.Count; i++)
                    {
                        content += "|-\n";

                        // First column with rowspan (only on first row)
                        if (i == 0)
                        {
                            var rowspan = awardType.Winners.Count > 1 ? $" rowspan=\"{awardType.Winners.Count}\"" : "";
                            content += $"|{rowspan} | ";

                            // Format based on heading/mode
                            if (heading.Heading == "WookieeProject Contributions")
                            {
                                content += FormatProjectLink(awardType.Type);
                            }
                            else if (subheading.Mode is TableMode.LongestStatusArticle)
                            {
                                content += FormatArticleLink(awardType.Type);
                            }
                            else
                            {
                                content += awardType.Type;
                            }

                            content += "\n";
                        }

                        // Place column (conditional based on TableMode)
                        if (subheading.Mode is not TableMode.MostDaysWithArticles and not TableMode.MVP and not TableMode.LongestStatusArticle)
                        {
                            content += $"| {i + 1}\n";
                        }

                        // Names column
                        content += "| \n";
                        content += FormatNamesList(awardType.Winners[i].Names, subheading.Mode);

                        // Count column
                        content += $"| {awardType.Winners[i].Count}\n";
                    }
                }

                // Closing table
                content += "|}\n\n";
            }
        }

        // Participation Awards section
        if (result.NominatorsWhoParticipatedButDidntPlace.Count > 0)
        {
            content += "== Participation Awards ==\n\n";
            foreach (var nominator in result.NominatorsWhoParticipatedButDidntPlace)
            {
                content += $"* {FormatNominatorLink(nominator)}\n";
            }
            content += "\n";
        }

        // New Projects section
        if (result.AddedProjects.Count > 0)
        {
            content += "== New Projects ==\n\n";
            var projectNumber = 1;
            foreach (var project in result.AddedProjects)
            {
                content += $"# {project.CreatedAt:d} - {FormatProjectLink(project.Name)}\n";
                projectNumber++;
            }
            content += "\n";
        }

        // Total First-Place Count section
        if (result.TotalFirstPlaceAwards > 0)
        {
            content += "== Total First-Place Count ==\n\n";
            content += $"This year, the total number of first-place awards is '''{result.TotalFirstPlaceAwards}'''.\n\n";
        }

        // Nominations With Most WookieeProjects section
        if (result.NominationsWithMostWookieeProjects.Count > 0)
        {
            content += "== Nominations With Most WookieeProjects ==\n\n";
            content += $"The following Status Article Nominations had the highest number of WookieeProjects, with {result.NominationsWithMostWookieeProjects.First().Projects!.Count()} projects total!\n\n";

            content += "{|{{Prettytable|style=margin:auto}}\n";
            content += "|+ Nominations With Most WookieeProjects\n";
            content += "! Article !! Nominators !! WookieeProjects\n";

            foreach (var nomination in result.NominationsWithMostWookieeProjects)
            {
                content += "|-\n";

                // Article column
                content += $"| {FormatArticleLink(nomination.ArticleName)}\n";

                // Nominators column
                content += "| \n";
                foreach (var nominator in nomination.Nominators!)
                {
                    content += $"* {FormatNominatorLink(nominator)}\n";
                }

                // WookieeProjects column
                content += "| \n";
                foreach (var project in nomination.Projects!)
                {
                    content += $"* {FormatProjectLink(project.Name)}\n";
                }
            }

            content += "|}\n\n";
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var fileName = $"award-generation-group-{id}.txt";

        return File(bytes, "text/plain; charset=utf-8", fileName);
    }

    private static string FormatArticleLink(string articleName)
    {
        // Strip nomination suffix and replace spaces with underscores
        var realArticleName = System.Text.RegularExpressions.Regex.Replace(articleName, @" \(.+ nomination\)$", "");
        var linkName = realArticleName.Replace(" ", "_");
        return $"[[{linkName}|{articleName}]]";
    }

    private static string FormatProjectLink(string projectName)
    {
        var linkName = projectName.Replace(" ", "_");
        return $"[[Wookieepedia:WookieeProject_{linkName}|{projectName}]]";
    }

    private static string FormatNominatorLink(Nominations.Nominators.Nominator nominator)
    {
        if (nominator.IsRedacted)
        {
            return "Redacted";
        }
        var linkName = nominator.Name.Replace(" ", "_");
        return $"{{{{U|{nominator.Name}}}}}";
    }

    private static string FormatNamesList(IList<WinnerNameViewModel> names, TableMode mode)
    {
        var result = "";
        foreach (var name in names)
        {
            if (name is WinnerNameViewModel.NominatorView nominator)
            {
                result += $"* {FormatNominatorLink(nominator.Nominator)}\n";
            }
            else if (name is WinnerNameViewModel.WookieeProject project)
            {
                result += $"* {FormatProjectLink(project.ProjectName)}\n";
            }
            else if (name is WinnerNameViewModel.Date date)
            {
                result += $"* {date.DateOfNomination.ToLongDateString()}\n";
            }
        }
        return result;
    }
}