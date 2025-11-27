using System.Text;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;

namespace WookiepediaStatusArticleData.Services.Awards;

public class WookieepediaExportService(WookiepediaDbContext db)
{
    public async Task<string> ExportToWookieepediaFormatAsync(
        AwardGenerationGroup group,
        CancellationToken cancellationToken
    )
    {
        // Get awards for Overall (All Articles), GA (Good Articles), and FA (Featured Articles)
        // We're looking for "Sheer Numbers" -> "All Nominators" category
        var overallAwards = await GetAwardsForCategoryAsync(
            group.Id,
            "Sheer Numbers",
            "All Nominators",
            "All Articles",
            cancellationToken
        );

        var gaAwards = await GetAwardsForCategoryAsync(
            group.Id,
            "Sheer Numbers",
            "All Nominators",
            $"{NominationType.Good.GetDisplayName()} Articles",
            cancellationToken
        );

        var faAwards = await GetAwardsForCategoryAsync(
            group.Id,
            "Sheer Numbers",
            "All Nominators",
            $"{NominationType.Featured.GetDisplayName()} Articles",
            cancellationToken
        );

        return GenerateWikitextTable(overallAwards, gaAwards, faAwards);
    }

    private async Task<List<PlacementGroup>> GetAwardsForCategoryAsync(
        int groupId,
        string heading,
        string subheading,
        string type,
        CancellationToken cancellationToken
    )
    {
        var awards = await db.Set<Award>()
            .Where(a => a.GenerationGroupId == groupId)
            .Where(a => a.Heading == heading)
            .Where(a => a.Subheading == subheading)
            .Where(a => a.Type == type)
            .Where(a => a.Placement != AwardPlacement.DidNotPlace)
            .Include(a => a.Nominator)
            .OrderBy(a => a.Placement)
            .ThenByDescending(a => a.Count)
            .ThenBy(a => a.Nominator!.Name)
            .ToListAsync(cancellationToken);

        // Group by placement
        return awards
            .GroupBy(a => a.Placement)
            .Select(g => new PlacementGroup
            {
                Placement = g.Key,
                Winners = g
                    .GroupBy(a => a.Count)
                    .OrderByDescending(cg => cg.Key)
                    .First() // Take the top count for this placement
                    .Select(a => new WinnerInfo
                    {
                        Name = a.Nominator!.Name,
                        Count = a.Count
                    })
                    .OrderBy(w => w.Name)
                    .ToList()
            })
            .ToList();
    }

    private string GenerateWikitextTable(
        List<PlacementGroup> overall,
        List<PlacementGroup> ga,
        List<PlacementGroup> fa
    )
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("{|{{Prettytable|style=margin:auto}}");
        sb.AppendLine("! !! Overall !! Count !! GA !! Count !! FA !! Count");
        sb.AppendLine("|-");

        // Process each placement (1st, 2nd, 3rd)
        var placements = new[] { AwardPlacement.First, AwardPlacement.Second, AwardPlacement.Third };
        
        foreach (var placement in placements)
        {
            var placementText = placement switch
            {
                AwardPlacement.First => "1st",
                AwardPlacement.Second => "2nd",
                AwardPlacement.Third => "3rd",
                _ => ""
            };

            var overallGroup = overall.FirstOrDefault(g => g.Placement == placement);
            var gaGroup = ga.FirstOrDefault(g => g.Placement == placement);
            var faGroup = fa.FirstOrDefault(g => g.Placement == placement);

            sb.Append($"|{placementText}||");
            
            // Overall column
            AppendWinners(sb, overallGroup?.Winners, placement == AwardPlacement.First);
            sb.Append("||");
            sb.Append(overallGroup?.Winners.FirstOrDefault()?.Count ?? 0);
            sb.Append("||");
            
            // GA column
            AppendWinners(sb, gaGroup?.Winners, placement == AwardPlacement.First);
            sb.Append("||");
            sb.Append(gaGroup?.Winners.FirstOrDefault()?.Count ?? 0);
            sb.Append("||");
            
            // FA column
            AppendWinners(sb, faGroup?.Winners, placement == AwardPlacement.First);
            sb.Append("||");
            sb.Append(faGroup?.Winners.FirstOrDefault()?.Count ?? 0);
            
            sb.AppendLine();
            sb.AppendLine("|-");
        }

        sb.AppendLine("|}");
        
        return sb.ToString();
    }

    private void AppendWinners(StringBuilder sb, List<WinnerInfo>? winners, bool isBold)
    {
        if (winners == null || winners.Count == 0)
        {
            return;
        }

        if (winners.Count == 1)
        {
            var winner = winners[0];
            if (isBold)
            {
                sb.Append($"'''{{{{U|{winner.Name}}}}}'''");
            }
            else
            {
                sb.Append($"{{{{U|{winner.Name}}}}}");
            }
        }
        else
        {
            // Multiple winners - format as a list without leading newline
            foreach (var winner in winners)
            {
                sb.AppendLine();
                if (isBold)
                {
                    sb.Append($"*'''{{{{U|{winner.Name}}}}}'''");
                }
                else
                {
                    sb.Append($"*{{{{U|{winner.Name}}}}}");
                }
            }
        }
    }

    private class PlacementGroup
    {
        public required AwardPlacement Placement { get; init; }
        public required List<WinnerInfo> Winners { get; init; }
    }

    private class WinnerInfo
    {
        public required string Name { get; init; }
        public required int Count { get; init; }
    }
}
