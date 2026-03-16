using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Projects;
using WookiepediaStatusArticleData.Services.Awards;

namespace WookiepediaStatusArticleData.Controllers;

[AllowAnonymous]
[Route("/")]
public class HomeController(WookiepediaDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] int? awardId,
        [FromServices] AwardsAggregationService awardsAggregationService,
        CancellationToken cancellationToken
    )
    {
        var groups = await db.Set<AwardGenerationGroup>()
            .OrderByDescending(g => g.StartedAt)
            .ThenByDescending(g => g.EndedAt)
            .ThenBy(g => g.Name)
            .ToListAsync(cancellationToken);

        var selectedGroup =
            awardId != null
                ? groups.SingleOrDefault(it => it.Id == awardId.Value)
                : groups.FirstOrDefault();

        if (selectedGroup == null)
            return View(
                new HomePageViewModel
                {
                    Groups = groups
                        .Select(g => new SelectListItem(g.Name, g.Id.ToString(), g.Id == awardId))
                        .ToList(),
                    Selected = null
                }
            );

        var result = await awardsAggregationService.RetrieveTablesAsync(
            selectedGroup,
            cancellationToken
        );
        // TODO: this is gross, fix later
        result.Groups = groups
            .Select(g => new SelectListItem(g.Name, g.Id.ToString(), g.Id == awardId))
            .ToList();

        // Get all unique nominators who participated in this award group
        var nominators = await db.Set<Award>()
            .Where(a => a.GenerationGroupId == selectedGroup.Id)
            .Include(a => a.Nominator)
            .Select(a => a.Nominator!.Name)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync(cancellationToken);

        result.Nominators = nominators
            .Select(name => new SelectListItem(name, name))
            .ToList();

        return View(result);
    }

    [HttpPost("nominator-first-place-awards")]
    public async Task<IActionResult> GetNominatorFirstPlaceAwards(
        [FromQuery] int? awardId,
        [FromForm] string nominatorName,
        [FromServices] AwardsAggregationService awardsAggregationService,
        CancellationToken cancellationToken
    )
    {
        var groups = await db.Set<AwardGenerationGroup>()
            .OrderByDescending(g => g.StartedAt)
            .ThenByDescending(g => g.EndedAt)
            .ThenBy(g => g.Name)
            .ToListAsync(cancellationToken);

        var selectedGroup =
            awardId != null
                ? groups.SingleOrDefault(it => it.Id == awardId.Value)
                : groups.FirstOrDefault();

        if (selectedGroup == null)
            return NotFound();

        var result = await awardsAggregationService.RetrieveTablesAsync(
            selectedGroup,
            cancellationToken
        );

        if (result.Selected == null)
            return NotFound();

        // Get all projects for code lookup
        var projects = await db.Set<Project>()
            .Where(p => !p.IsArchived)
            .ToListAsync(cancellationToken);
        var projectCodeMap = projects.ToDictionary(p => p.Name, p => p.Code);

        // Filter for first-place awards where this nominator appears
        var firstPlaceAwards = new List<NominatorFirstPlaceAwardDto>();

        foreach (var heading in result.Selected.AwardHeadings)
        {
            foreach (var subheading in heading.Subheadings)
            {
                foreach (var award in subheading.Awards)
                {
                    // First place = first WinnerViewModel (highest count)
                    if (award.Winners.Count == 0)
                        continue;

                    var firstPlace = award.Winners[0];

                    // Check if nominator is in this first-place group
                    var nominatorInFirstPlace = firstPlace
                        .Names.OfType<WinnerNameViewModel.NominatorView>()
                        .Any(n =>
                            n.Nominator.Name.Equals(
                                nominatorName,
                                StringComparison.OrdinalIgnoreCase
                            )
                        );

                    if (!nominatorInFirstPlace)
                        continue;

                    // Determine ProjectCode
                    string? projectCode = null;
                    if (heading.Heading == "WookieeProject Contributions")
                    {
                        // For WookieeProject Contributions, the Type is the project name
                        projectCodeMap.TryGetValue(award.Type, out projectCode);
                    }

                    firstPlaceAwards.Add(
                        new NominatorFirstPlaceAwardDto
                        {
                            Code = award.Code ?? "",
                            ProjectCode = projectCode,
                            Year = selectedGroup.Name
                        }
                    );
                }
            }
        }

        return PartialView("_NominatorFirstPlaceTemplate", firstPlaceAwards);
    }

    [Route("/home/error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}