using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Services.Nominations;

namespace WookiepediaStatusArticleData.Services.Awards.ProjectAwardCalculations;

public class NominationsByProjectAwardGenerator(WookiepediaDbContext db) : IProjectAwardCalculation
{
    public string Name => "Most Status Articles";

    public async Task<IList<ProjectCountProjection>> GenerateAsync(
        AwardGenerationGroup awardGenerationGroup,
        CancellationToken cancellationToken
    )
    {
        return await db.Set<Nomination>()
            .ForAwardCalculations(awardGenerationGroup)
            .GroupByProject()
            .Select(it => new ProjectCountProjection
            {
                Project = it.Key,
                Count = it.Count()
            })
            .OrderByDescending(it => it.Count)
            .ToListAsync(cancellationToken);
    }
}