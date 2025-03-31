using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Nominations.Projects;
using WookiepediaStatusArticleData.Services.Awards;
using WookiepediaStatusArticleData.Services.Awards.OnTheFlyCalculations;
using WookiepediaStatusArticleData.Services.Nominations;

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
                NominatorsWhoParticipatedButDidntPlace = await LookupNominatorsWhoParticipatedButDidntPlace(
                    awardHeadings,
                    selectedGroup
                ),
                AddedProjects = await db.Set<Project>()
                    .Where(it => !it.IsArchived)
                    .Where(it => selectedGroup.StartedAt <= it.CreatedAt && it.CreatedAt <= selectedGroup.EndedAt)
                    .OrderBy(it => it.CreatedAt)
                    .ToListAsync(cancellationToken),
                TotalFirstPlaceAwards = await db.Set<Award>()
                    .Where(it => it.GenerationGroupId == selectedGroup.Id)
                    .CountAsync(it => it.Placement == AwardPlacement.First, cancellationToken)
            }
        );
    }
    
    private async Task<IList<Nominator>> LookupNominatorsWhoParticipatedButDidntPlace(
        IList<AwardHeadingViewModel> awardHeadings,
        AwardGenerationGroup selectedGroup
    )
    {
        var allNominatorsWhoPlaced = awardHeadings
            .SelectMany(it => it.Subheadings)
            .SelectMany(it => it.Awards)
            .SelectMany(it => it.Winners)
            .SelectMany(it => it.Names)
            .ToHashSet();

        // notice that we don't care what the outcome of the nomination is. this is a participation award.
        // regardless of whether your nomination was successful or not, we count it as _participation_.
        var allNominations = db.Set<Nomination>()
            .EndedWithinTimeframe(selectedGroup.StartedAt, selectedGroup.EndedAt)
            .WithoutBannedNominators(selectedGroup.CreatedAt)
            .AsAsyncEnumerable();
        
        IList<Nominator> allNominatorsWhoParticipatedButDidntPlace = [];

        // TODO: THIS IS GROSS!! The filter in WithoutBannedNominators is done in a `Include` call.
        //       That filter isn't considered when SelectMany is called. so, I do this instead.
        //       Would love a better way to do this, though.
        await foreach (var nomination in allNominations)
        {
            foreach (var nominator in nomination.Nominators!)
            {
                if (!allNominatorsWhoPlaced.Contains(nominator.Name))
                {
                    allNominatorsWhoParticipatedButDidntPlace.Add(nominator);
                }
            }
        }
        
        allNominatorsWhoParticipatedButDidntPlace = allNominatorsWhoParticipatedButDidntPlace
            .Distinct()
            .OrderBy(it => it.Name)
            .ToList();

        return allNominatorsWhoParticipatedButDidntPlace
            .Where(it => !allNominatorsWhoPlaced.Contains(it.Name))
            .ToList();
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