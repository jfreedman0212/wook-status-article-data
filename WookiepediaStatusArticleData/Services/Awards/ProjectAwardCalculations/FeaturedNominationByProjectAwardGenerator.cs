using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Services.Nominations;

namespace WookiepediaStatusArticleData.Services.Awards.ProjectAwardCalculations;

public class FeaturedNominationByProjectAwardGenerator(WookiepediaDbContext db) : IProjectAwardCalculation
{
    public string Name => "Most Featured Nominations";
    
    public async Task<IList<ProjectCountProjection>> GenerateAsync(AwardGenerationGroup awardGenerationGroup, CancellationToken cancellationToken)
    {
        return await db.Set<Nomination>()
            .ForAwardCalculations(awardGenerationGroup)
            .WithType(NominationType.Featured)
            .GroupByProject()
            .Select(it => new ProjectCountProjection
            {
                Project = it.Key,
                Count = it.Count()
            })
            .ToListAsync(cancellationToken);
            
    }
}