using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Awards;

namespace WookiepediaStatusArticleData.Services.Awards;

public class TopAwardsLookup(WookiepediaDbContext db)
{
    public async Task<IList<AwardHeadingViewModel>> LookupAsync(
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
                    "Count"
                from
                    RankedValues
                where
                    "Rank" <= {numberOfResults}
                """
            )
            .ToListAsync(cancellationToken);

        return rawResults
            .GroupBy(it => (it.Heading, it.Subheading, it.Type))
            .Select(group => new AwardViewModel
            {
                Order = group.FirstOrDefault()?.Id ?? 0,
                Heading = group.Key.Heading,
                Subheading = group.Key.Subheading,
                Type = group.Key.Type,
                Winners = group
                    .GroupBy(it => it.Count)
                    .Select(it => new WinnerViewModel
                    {
                        Names = it.Select(x => x.NominatorName).Order().ToList(),
                        Count = it.Key
                    })
                    .ToList()
            })
            .OrderBy(it => it.Order)
            .GroupBy(it => it.Heading)
            .Select(it => new AwardHeadingViewModel
            {
                Heading = it.Key,
                Subheadings = it
                    .OrderBy(sh => sh.Subheading)
                    .GroupBy(sh => sh.Subheading)
                    .Select(sh => new SubheadingAwardViewModel
                    {
                        Subheading = sh.Key,
                        Awards = sh.ToList()
                    })
                    .ToList()
            })
            .ToList();
    }
}

class QueryResult
{
    public required int Id { get; set; }
    public required string Heading { get; set; }
    public required string Subheading { get; set; }
    public required string Type { get; set; }
    public required int NominatorId { get; set; }
    public required string NominatorName { get; set; }
    public required int Count { get; set; }
}