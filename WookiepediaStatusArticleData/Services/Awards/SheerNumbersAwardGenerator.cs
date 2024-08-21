using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Nominations;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Services.Nominations;

namespace WookiepediaStatusArticleData.Services.Awards;

public class SheerNumbersAwardGenerator(WookiepediaDbContext db) : IAwardGenerator
{
    public async Task<IList<Award>> GenerateAsync(AwardGenerationGroup generationGroup, CancellationToken cancellationToken)
    {
        var x = await db.Set<Nomination>()
            .Filter(new NominationQuery
            {
                Type = NominationType.Comprehensive,
                Outcome = Outcome.Successful
            })
            .WithinRange(generationGroup.StartedAt, generationGroup.EndedAt)
            .SelectMany(it => it.Nominators!, (nomination, nominator) => new
            {
                Nomination = nomination, Nominator = nominator
            })
            .GroupBy(it => it.Nominator)
            .Select(it => new
            {
                Nominator = it.Key,
                Count = it.Count()
            })
            .ToListAsync(cancellationToken);

        return x
            .Select(it => new Award
            {
                GenerationGroup = generationGroup,
                Type = 1,
                Nominator = it.Nominator,
                Count = it.Count
                
            })
            .ToList();
    }
}