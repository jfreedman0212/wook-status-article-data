using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Nominations.Projects;
using WookiepediaStatusArticleData.Services.Awards;
using WookiepediaStatusArticleData.Services.Awards.OnTheFlyCalculations;

namespace WookiepediaStatusArticleData.Controllers;

[AllowAnonymous]
[Route("/")]
public class HomeController(WookiepediaDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] int? awardId,
        [FromServices] TopAwardsLookup topAwardsLookup,
        [FromServices] IEnumerable<IOnTheFlyCalculation> onTheFlyCalculations,
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

        var awardHeadings = await topAwardsLookup.LookupAsync(selectedGroup, cancellationToken);

        var additionalAwardsHeadings = new AwardHeadingViewModel
        {
            Heading = "Additional Awards",
            Subheadings = []
        };

        foreach (var calculation in onTheFlyCalculations)
        {
            additionalAwardsHeadings.Subheadings.AddRange(
                await calculation.CalculateAsync(selectedGroup, cancellationToken)
            );
        }

        if (additionalAwardsHeadings.Subheadings.Count > 0)
        {
            awardHeadings.Add(additionalAwardsHeadings);   
        }

        return View(
            new HomePageViewModel
            {
                Groups = groups
                    .Select(g => new SelectListItem(g.Name, g.Id.ToString(), g.Id == awardId))
                    .ToList(),
                Selected = new AwardGenerationGroupDetailViewModel
                {
                    Id = selectedGroup.Id,
                    Name = selectedGroup.Name,
                    StartedAt = selectedGroup.StartedAt,
                    EndedAt = selectedGroup.EndedAt,
                    AwardHeadings = awardHeadings
                },
                NominatorsWhoParticipatedButDidntPlace = await db.Set<Award>()
                    .Where(it => it.GenerationGroupId == selectedGroup.Id)
                    .Where(it => !it.Nominator!.Attributes!.Any(
                        attr => attr.AttributeName == NominatorAttributeType.Banned
                                && attr.EffectiveAt <= selectedGroup.CreatedAt
                                && (attr.EffectiveEndAt == null || selectedGroup.CreatedAt <= attr.EffectiveEndAt)
                    ))
                    .GroupBy(a => a.Nominator)
                    .Select(g => new 
                    {
                        Nominator = g.Key,
                        PlacedCount = g.Count(a => a.Placement != AwardPlacement.DidNotPlace)
                    })
                    .Where(x => x.PlacedCount == 0)
                    .Select(it => it.Nominator!)
                    .OrderBy(it => it.Name)
                    .Distinct()
                    .ToListAsync(cancellationToken),
                AddedProjects = await db.Set<Project>()
                    .Where(it => !it.IsArchived)
                    .Where(it => selectedGroup.StartedAt <= it.CreatedAt && it.CreatedAt <= selectedGroup.EndedAt)
                    .OrderBy(it => it.CreatedAt)
                    .ToListAsync(cancellationToken)
            }
        );
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