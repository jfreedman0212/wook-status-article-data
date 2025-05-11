using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Services.Awards.NominatorAwardCalculations;
using WookiepediaStatusArticleData.Services.Awards.ProjectAwardCalculations;

namespace WookiepediaStatusArticleData.Services.Awards;

public class GenerateAwardsAction(
    WookiepediaDbContext db,
    IEnumerable<INominatorAwardCalculation> awardGenerators,
    IEnumerable<IProjectAwardCalculation> projectAwardCalculations,
    NominatorAwardPlacementCalculation placementCalculation
)
{
    public async Task RefreshAsync(AwardGenerationGroup awardGenerationGroup, CancellationToken cancellationToken)
    {
        // clear out current awards
        awardGenerationGroup.Awards = [];
        awardGenerationGroup.ProjectAwards = [];
        await db.Set<Award>()
            .Where(g => g.GenerationGroupId == awardGenerationGroup.Id)
            .ExecuteDeleteAsync(cancellationToken);
        await db.Set<ProjectAward>()
            .Where(g => g.GenerationGroupId == awardGenerationGroup.Id)
            .ExecuteDeleteAsync(cancellationToken);

        awardGenerationGroup.UpdatedAt = DateTime.UtcNow;

        await ExecuteAsync(awardGenerationGroup, cancellationToken);
    }

    public async Task ExecuteAsync(AwardGenerationGroup awardGenerationGroup, CancellationToken cancellationToken)
    {
        await GenerateAwards(awardGenerationGroup, cancellationToken);
        // flush changes first so the rows are in the DB
        await db.SaveChangesAsync(cancellationToken);
        // then, run the fancy SQL to determine placement for each nominator
        await DeterminePlacement(awardGenerationGroup, cancellationToken);
        // flush the changes again so the placement is updated
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task GenerateAwards(
        AwardGenerationGroup group,
        CancellationToken cancellationToken
    )
    {
        foreach (var awardGenerator in awardGenerators)
        {
            var awards = await awardGenerator.GenerateAsync(group, cancellationToken);
            group.Awards!.AddRange(awards);
        }

        foreach (var calculation in projectAwardCalculations)
        {
            var projectAwardCounts = await calculation.GenerateAsync(group, cancellationToken);
            group.ProjectAwards!.AddRange(
                projectAwardCounts.Select(it => new ProjectAward
                {
                    GenerationGroup = group,
                    Heading = "WookieeProjects",
                    Type = calculation.Name,
                    Project = it.Project,
                    Count = it.Count
                })
            );
        }
    }

    private async Task DeterminePlacement(AwardGenerationGroup group, CancellationToken cancellationToken)
    {
        var placementProjections = await placementCalculation.CalculatePlacementAsync(
            group.Id,
            3,
            cancellationToken
        );

        foreach (var placementProjection in placementProjections)
        {
            var award = group.Awards!.FirstOrDefault(it => it.Id == placementProjection.Id);

            if (award == null) continue;

            award.Placement = placementProjection.Rank switch
            {
                1 => AwardPlacement.First,
                2 => AwardPlacement.Second,
                3 => AwardPlacement.Third,
                _ => AwardPlacement.DidNotPlace
            };
        }
    }
}
