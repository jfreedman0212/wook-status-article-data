using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Awards;

namespace WookiepediaStatusArticleData.Services.Awards;

public class TopAwardsLookup(WookiepediaDbContext db)
{
    public async Task<IList<AwardViewModel>> LookupAsync(
        int groupId,
        int numberOfResults,
        CancellationToken cancellationToken
    )
    {
        var rawResults = await db.Database
            .SqlQuery<QueryResult>( 
                // language=SQL
                $"""
                 WITH RankedValues AS (
                    select 
                        a.id as "Id",
                        a.type as "Type",
                        n.id as "NominatorId",
                        n.name as "NominatorName",
                        a.count as "Count",
                        dense_rank() over (order by a.count desc) AS "Rank"
                    from 
                        award_generation_groups g
                    join 
                        awards a on a.generation_group_id = g.id
                    join
                        nominators n on n.id = a.nominator_id
                    where
                        g.id = {groupId}
                    order by
                        a.type, a.count desc
                )
                select
                    "Id",
                    "Type",
                    "NominatorId",
                    "NominatorName",
                    "Count"
                from
                    RankedValues
                where
                    "Rank" <= {numberOfResults}
                """
            )
            .ToListAsync(cancellationToken);

        return rawResults
            .GroupBy(it => it.Type)
            .Select(group => new AwardViewModel
            {
                Type = group.Key,
                Winners = group
                    .GroupBy(it => it.Count)
                    .Select(it => new WinnerViewModel
                    {
                        Names = it.Select(x => x.NominatorName).ToList(),
                        Count = it.Key
                    })
                    .ToList()
            })
            .ToList();
    }
}

class QueryResult
{
    public required int Id { get; set; }
    public required int Type { get; set; }
    public required int NominatorId { get; set; }
    public required string NominatorName { get; set; }
    public required int Count { get; set; }
}