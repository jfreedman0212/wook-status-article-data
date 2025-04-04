using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Services.Awards;

public static class AwardQueryableExtensions
{
    public static IQueryable<Award> WithoutBannedNominators(this IQueryable<Award> self, DateTime createdAt)
    {
        return self.Where(it => !it.Nominator!.Attributes!.Any(
            attr => attr.AttributeName == NominatorAttributeType.Banned
                    && attr.EffectiveAt <= createdAt
                    && (attr.EffectiveEndAt == null || createdAt <= attr.EffectiveEndAt)
        ));
    }
    
    public static IQueryable<Award> WithPanelistsOnly(
        this IQueryable<Award> self,
        DateTime startedAt,
        DateTime endedAt
    )
    {
        return self.Where(it => it.Nominator!.Attributes!.Any(
            attr => (attr.AttributeName == NominatorAttributeType.Inquisitor
                     || attr.AttributeName == NominatorAttributeType.AcMember)
                    // if it overlaps with the year at all, treat them as if they have been a panelist the whole year
                    && attr.EffectiveAt <= endedAt
                    && (attr.EffectiveEndAt == null || startedAt <= attr.EffectiveEndAt)
        ));
    }

    public static IQueryable<Award> WithNonPanelistsOnly(
        this IQueryable<Award> self,
        DateTime startedAt,
        DateTime endedAt
    )
    {
        return self.Where(it =>
            !it.Nominator!.Attributes!.Any(
                attr => (attr.AttributeName == NominatorAttributeType.Inquisitor
                         || attr.AttributeName == NominatorAttributeType.AcMember)
                        // if it overlaps with the year at all, treat them as if they have been a panelist the whole year
                        && attr.EffectiveAt <= endedAt
                        && (attr.EffectiveEndAt == null || startedAt <= attr.EffectiveEndAt)
            )
        );
    }

    public static IQueryable<NominatorPointsProjection> MostValuableNominatorPoints(this IQueryable<Award> self)
    {
        return self.GroupBy(it => it.Nominator!)
            .Select(it => new NominatorPointsProjection
            {
                Nominator = it.Key,
                Points = it.Sum(
                    a => a.Placement == AwardPlacement.First ? 3 :
                        a.Placement == AwardPlacement.Second ? 2 :
                        a.Placement == AwardPlacement.Third ? 1 :
                        0
                )
            });
    }
}

public class NominatorPointsProjection
{
    public required Nominator Nominator { get; init; }
    public required int Points { get; init; }
}