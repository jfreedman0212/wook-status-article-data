using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;

namespace WookiepediaStatusArticleData.Services.Awards;

public class NominatorAwardPlacementCalculation(WookiepediaDbContext db)
{
    public async Task<IList<PlacementProjection>> CalculatePlacementAsync(
        int groupId,
        int numberOfResults,
        CancellationToken cancellationToken
    )
    {
        return await db.Database
            .SqlQuery<PlacementProjection>( 
                // language=SQL
                $"""
                  WITH RankedValues AS (
                     select 
                         a.id as "Id",
                         a.heading as "Heading",
                         a.subheading as "Subheading",
                         a.type as "Type",
                         n.id as "NominatorId",
                         n.name as "NominatorName",
                         a.count as "Count",
                         dense_rank() over (partition by a.heading, a.subheading, a.type order by a.count desc) AS "Rank"
                     from 
                         award_generation_groups g
                     join 
                         awards a on a.generation_group_id = g.id
                     join
                         nominators n on n.id = a.nominator_id
                     where
                         g.id = {groupId}
                     order by
                         a.heading, a.subheading, a.type, a.count desc
                 )
                 select
                     "Id",
                     "Heading",
                     "Subheading",
                     "Type",
                     "NominatorId",
                     "NominatorName",
                     "Count",
                     "Rank"
                 from
                     RankedValues
                 where
                     "Rank" <= {numberOfResults}
                 """
            )
            .ToListAsync(cancellationToken);
    }
}

[UsedImplicitly]
public class PlacementProjection
{
    public required int Id { get; set; }
    public required string Heading { get; set; }
    public required string Subheading { get; set; }
    public required string Type { get; set; }
    public required int NominatorId { get; set; }
    public required string NominatorName { get; set; }
    public required int Count { get; set; }
    public required int Rank { get; set; }
}