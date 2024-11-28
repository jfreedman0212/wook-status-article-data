using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Services.Awards;

public class StaticAwardGenerator(WookiepediaDbContext db) : IAwardGenerator
{
    private readonly IList<IQueryBuilder> _awardQueryBuilders = 
    [
        new NominationQueryBuilder("Sheer Numbers", db)
            .WithType(NominationType.Comprehensive),
        new NominationQueryBuilder("Inquisitorius and AgriCorp Overall", db)
            .WithNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember),
        new NominationQueryBuilder("Inquisitorius and AgriCorp GA", db)
            .WithType(NominationType.Good)
            .WithNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember),
        new NominationQueryBuilder("Inquisitorius and AgriCorp FA", db)
            .WithType(NominationType.Featured)
            .WithNominatorAttribute(NominatorAttributeType.Inquisitor, NominatorAttributeType.AcMember),
        new NominationQueryBuilder("Canon Comprehensive", db)
            .WithType(NominationType.Comprehensive)
            .WithContinuity(Continuity.Canon),
        new NominationQueryBuilder("Legends Comprehensive", db)
            .WithType(NominationType.Comprehensive)
            .WithContinuity(Continuity.Legends),
        new NominationQueryBuilder("OOU Comprehensive", db)
            .WithType(NominationType.Comprehensive)
            .WithContinuity(Continuity.OutOfUniverse),
    ];
    
    public async Task<IList<Award>> GenerateAsync(AwardGenerationGroup generationGroup, CancellationToken cancellationToken)
    {
        var list = new List<Award>();

        foreach (var queryBuilder in _awardQueryBuilders)
        {
            list.AddRange(await queryBuilder.BuildAsync(generationGroup, cancellationToken));
        }

        return list;
    }
}