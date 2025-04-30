using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Services.Awards;

namespace WookiepediaStatusArticleData.Services.Nominations;

public static class NominatorNominationProjectionQueryableExtensions
{
    public static IQueryable<NominatorNominationProjection> WithPanelistsOnly(
        this IQueryable<NominatorNominationProjection> self,
        DateTime startedAt,
        DateTime endedAt
    )
    {
        return self.Where(it => it.Nominator.Attributes!.Any(
            attr => (attr.AttributeName == NominatorAttributeType.Inquisitor
                     || attr.AttributeName == NominatorAttributeType.AcMember)
                    // if it overlaps with the year at all, treat them as if they have been a panelist the whole year
                    && attr.EffectiveAt <= endedAt
                    && (attr.EffectiveEndAt == null || startedAt <= attr.EffectiveEndAt)
        ));
    }

    public static IQueryable<NominatorNominationProjection> WithNonPanelistsOnly(
        this IQueryable<NominatorNominationProjection> self,
        DateTime startedAt,
        DateTime endedAt
    )
    {
        return self.Where(it =>
            !it.Nominator.Attributes!.Any(
                attr => (attr.AttributeName == NominatorAttributeType.Inquisitor
                         || attr.AttributeName == NominatorAttributeType.AcMember)
                        // if it overlaps with the year at all, treat them as if they have been a panelist the whole year
                        && attr.EffectiveAt <= endedAt
                        && (attr.EffectiveEndAt == null || startedAt <= attr.EffectiveEndAt)
            )
        );
    }

    public static IQueryable<NominatorNominationProjection> WithoutBannedNominators(
        this IQueryable<NominatorNominationProjection> self,
        DateTime createdAt
    )
    {
        // if the nominator is banned at the time of generation, do not include them in the count
        return self
            .Where(it => !it.Nominator.Attributes!.Any(
                attr => attr.AttributeName == NominatorAttributeType.Banned
                        && attr.EffectiveAt <= createdAt
                        && (attr.EffectiveEndAt == null || createdAt <= attr.EffectiveEndAt)
            ));
    }

    // TODO: this is a duplicate of the same query on the NominationQueryableExtensions.
    //  it only exists to allow us to make this same query later in the process.
    public static IQueryable<NominatorNominationProjection> EndedWithinTimeframe(
        this IQueryable<NominatorNominationProjection> self,
        DateTime startedAt,
        DateTime endedAt
    )
    {
        // we only care about nominations that ENDED within the timeframe of this generation group
        return self.Where(it =>
            it.Nomination.EndedAt != null
            && startedAt <= it.Nomination.EndedAt
            && it.Nomination.EndedAt <= endedAt
        );
    }

    public static IQueryable<NominatorCountProjection> CountByNumberOfArticles(
        this IQueryable<NominatorNominationProjection> self
    )
    {
        return self
            .GroupBy(it => it.Nominator)
            .Select(it => new NominatorCountProjection
            {
                Nominator = it.Key,
                Count = it.Count()
            });
    }

    public static IQueryable<NominatorCountProjection> CountByNumberOfUniqueProjects(
        this IQueryable<NominatorNominationProjection> self
    )
    {
        return self
            .GroupBy(it => it.Nominator)
            .Select(it => new NominatorCountProjection
            {
                Nominator = it.Key,
                Count = it.SelectMany(p => p.Nomination.Projects!).Distinct().Count()
            });
    }

    public static IQueryable<NominatorCountProjection> CountByJocastaBotPoints(
        this IQueryable<NominatorNominationProjection> self
    )
    {
        return self
            .GroupBy(it => it.Nominator)
            .Select(it => new NominatorCountProjection
            {
                Nominator = it.Key,
                // can't do a switch expression to generate a case expression. this is the next best thing,
                // but it's ugly :(
                Count = it.Sum(p =>
                    p.Nomination.Type == NominationType.Comprehensive ? 1 :
                    p.Nomination.Type == NominationType.Good ? 3 :
                    p.Nomination.Type == NominationType.Featured ? 5 :
                    0)
            });
    }
}

public class NominatorCountProjection
{
    public required Nominator Nominator { get; init; }
    public required int Count { get; init; }
}