using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Services.Nominations;

namespace WookiepediaStatusArticleData.Services.Awards.ProjectAwardCalculations;

public class PointsByProjectAwardGenerator(WookiepediaDbContext db) : IProjectAwardCalculation
{
    public string Name => "Highest Score";
    
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
                Count = it.Sum(p =>
                    p.Type == NominationType.Comprehensive ? 1 :
                    p.Type == NominationType.Good ? 3 :
                    p.Type == NominationType.Featured ? 5 :
                    0)
            })
            .OrderByDescending(it => it.Count)
            .ToListAsync(cancellationToken);
    }
}