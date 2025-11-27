using System.Text;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;

namespace WookiepediaStatusArticleData.Services.Awards;

public class WookieepediaExportService(TopAwardsLookup topAwardsLookup)
{
    public async Task<string> ExportToWookieepediaFormatAsync(
        AwardGenerationGroup group,
        CancellationToken cancellationToken
    )
    {
        // Use the same logic as HomeController to get award data
        var awardHeadings = await topAwardsLookup.LookupAsync(group, cancellationToken);

        // Extract the specific categories we need for the export
        var overallAwards = GetAwardsForCategory(
            awardHeadings,
            "Sheer Numbers",
            "Non-Panelist",
            "All Articles"
        );

        var gaAwards = GetAwardsForCategory(
            awardHeadings,
            "Sheer Numbers",
            "Non-Panelist",
            $"{NominationType.Good.GetDisplayName()} Articles"
        );

        var faAwards = GetAwardsForCategory(
            awardHeadings,
            "Sheer Numbers",
            "Non-Panelist",
            $"{NominationType.Featured.GetDisplayName()} Articles"
        );

        return GenerateWikitextTable(overallAwards, gaAwards, faAwards);
    }

    private List<PlacementGroup> GetAwardsForCategory(
        List<AwardHeadingViewModel> awardHeadings,
        string heading,
        string subheading,
        string type
    )
    {
        // Find the specific award category in the already-loaded data
        var award = awardHeadings
            .FirstOrDefault(h => h.Heading == heading)?
            .Subheadings
            .FirstOrDefault(s => s.Subheading == subheading)?
            .Awards
            .FirstOrDefault(a => a.Type == type);

        if (award == null)
        {
            return [];
        }

        // Convert the winners to placement groups
        // TopAwardsLookup already orders winners by count descending,
        // so the first group is 1st place, second is 2nd place, etc.
        var placementGroups = new List<PlacementGroup>();

        for (int i = 0; i < Math.Min(award.Winners.Count, 3); i++)
        {
            var placement = i switch
            {
                0 => AwardPlacement.First,
                1 => AwardPlacement.Second,
                2 => AwardPlacement.Third,
                _ => AwardPlacement.DidNotPlace
            };

            var winnerGroup = award.Winners[i];
            var winnerInfos = winnerGroup.Names
                .OfType<WinnerNameViewModel.NominatorView>()
                .Select(n => new WinnerInfo
                {
                    Name = n.Nominator.Name,
                    Count = winnerGroup.Count
                })
                .OrderBy(w => w.Name)
                .ToList();

            if (winnerInfos.Count > 0)
            {
                placementGroups.Add(new PlacementGroup
                {
                    Placement = placement,
                    Winners = winnerInfos
                });
            }
        }

        return placementGroups;
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
