using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;

namespace WookiepediaStatusArticleData.Services.Awards;

public class SheerNumbersAwardGenerator(WookiepediaDbContext db) : IAwardGenerator
{
    public async Task<IList<Award>> GenerateAsync(AwardGenerationGroup generationGroup, CancellationToken cancellationToken)
    {
        return await new NominationQueryBuilder(db, generationGroup)
            .WithType(NominationType.Comprehensive)
            .BuildAsync(1, cancellationToken);
    }
}
