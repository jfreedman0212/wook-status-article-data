using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;

namespace WookiepediaStatusArticleData.Services.Awards;

public class TopProjectAwardsLookup(WookiepediaDbContext db)
{
    public async Task<List<SubheadingAwardViewModel>> LookupAsync(
        int groupId,
        int numberOfResults,
        CancellationToken cancellationToken
    )
    {
        var rawResults = await db.Database
            .SqlQuery<ProjectQueryResult>(
                // language=SQL
                $"""
                 WITH RankedValues AS (
                    select 
                        a.id as "Id",
                        a.heading as "Heading",
                        a.type as "Type",
                        p.id as "ProjectId",
                        p.name as "ProjectName",
                        a.count as "Count",
                        dense_rank() over (partition by a.heading, a.type order by a.count desc) AS "Rank"
                    from 
                        award_generation_groups g
                    join 
                        project_awards a on a.generation_group_id = g.id
                    join
                        projects p on p.id = a.project_id
                    where
                        g.id = {groupId}
                    order by
                        a.heading, a.type, a.count desc
                )
                select
                    "Id",
                    "Heading",
                    "Type",
                    "ProjectId",
                    "ProjectName",
                    "Count"
                from
                    RankedValues
                where
                    "Rank" <= {numberOfResults}
                """
            )
            .ToListAsync(cancellationToken);

        return rawResults
            .GroupBy(it => (it.Heading, it.Type))
            .Select(group => new AwardViewModel
            {
                Order = group.FirstOrDefault()?.Id ?? 0,
                Heading = "",
                Subheading = group.Key.Heading,
                Type = group.Key.Type,
                Winners = group
                    .OrderByDescending(it => it.Count)
                    .GroupBy(it => it.Count)
                    .Select((it, index) => new WinnerViewModel
                    {
                        Names = it
                            .OrderBy(x => x.ProjectName)
                            .Select(x => new WinnerNameViewModel.WookieeProject(x.ProjectName))
                            .ToList<WinnerNameViewModel>(),
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
                        Mode = TableMode.WookieeProject,
                        Subheading = sh.Key,
                        Awards = sh.ToList()
                    })
                    .ToList()
            })
            .SelectMany(it => it.Subheadings)
            .ToList();
    }
}

class ProjectQueryResult
{
    public required int Id { get; set; }
    public required string Heading { get; set; }
    public required string Type { get; set; }
    public required int ProjectId { get; set; }
    public required string ProjectName { get; set; }
    public required int Count { get; set; }
}