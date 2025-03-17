using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Services.Nominations;

namespace WookiepediaStatusArticleData.Services.Awards.ProjectAwardCalculations;

public class NominatorsByProjectAwardGenerator(WookiepediaDbContext db) : IProjectAwardCalculation
{
    public string Name => "Most Participants per Project";
    
    public async Task<IList<ProjectCountProjection>> GenerateAsync(AwardGenerationGroup awardGenerationGroup, CancellationToken cancellationToken)
    {
        return await db.Set<Nomination>()
            .ForAwardCalculations(awardGenerationGroup)
            .GroupByProject()
            .Select(it => new ProjectCountProjection
            {
                Project = it.Key,
                Count = it
                    .SelectMany(nom => nom.Nominators!.Select(nominator => nominator.Id))
                    .Distinct()
                    .Count(),
            })
            .ToListAsync(cancellationToken);
    }
}