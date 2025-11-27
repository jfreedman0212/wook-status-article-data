using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Nominations.Projects;
using WookiepediaStatusArticleData.Services.Awards.OnTheFlyCalculations;
using WookiepediaStatusArticleData.Services.Nominations;

namespace WookiepediaStatusArticleData.Services.Awards;

public class AwardGenerationGroupDetailService(
    WookiepediaDbContext db,
    TopAwardsLookup topAwardsLookup,
    IEnumerable<IOnTheFlyCalculation> onTheFlyCalculations
)
{
    public async Task<AwardGenerationGroupDetailViewModel> GetDetailAsync(
        AwardGenerationGroup selectedGroup,
        CancellationToken cancellationToken
    )
    {
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

        var largestProjectCount = await db.Set<Nomination>()
            .ForAwardCalculations(selectedGroup)
            .Select(it => new
            {
                Nomination = it,
                ProjectsCount = it.Projects!.Count,
            })
            .MaxAsync(it => it.ProjectsCount, cancellationToken);

        var nominationsWithMostWookieeProjects = await db.Set<Nomination>()
            .Include(it => it.Projects!)
            .ForAwardCalculations(selectedGroup)
            .Select(it => new
            {
                Nomination = it,
                ProjectsCount = it.Projects!.Count,
            })
            .OrderByDescending(it => it.ProjectsCount)
            .Where(it => it.ProjectsCount == largestProjectCount)
            .Select(it => it.Nomination)
            .ToListAsync(cancellationToken);

        return new AwardGenerationGroupDetailViewModel
        {
            Id = selectedGroup.Id,
            Name = selectedGroup.Name,
            StartedAt = selectedGroup.StartedAt,
            EndedAt = selectedGroup.EndedAt,
            AwardHeadings = awardHeadings,
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
                .CountAsync(it => it.Placement == AwardPlacement.First, cancellationToken),
            NominationsWithMostWookieeProjects = nominationsWithMostWookieeProjects
        };
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
            .Where(it => it is WinnerNameViewModel.NominatorView)
            .Select(it => (it as WinnerNameViewModel.NominatorView)!.Nominator.Name)
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
}
