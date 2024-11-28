using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Services.Awards;

public class InqAndAgriCorpOverallAwardGenerator(WookiepediaDbContext db) : IAwardGenerator
{
    public async Task<IList<Award>> GenerateAsync(AwardGenerationGroup generationGroup, CancellationToken cancellationToken)
    {
        return await new NominationQueryBuilder(db, generationGroup)
            .WithNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember)
            .BuildAsync("Inquisitorius and AgriCorp Overall", cancellationToken);
    }
}