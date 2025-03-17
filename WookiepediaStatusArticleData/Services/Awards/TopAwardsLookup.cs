using WookiepediaStatusArticleData.Models.Awards;

namespace WookiepediaStatusArticleData.Services.Awards;

public class TopAwardsLookup(NominatorAwardPlacementCalculation placementCalculation)
{
    public async Task<List<AwardHeadingViewModel>> LookupAsync(
        int groupId,
        int numberOfResults,
        CancellationToken cancellationToken
    )
    {
        var rawResults = await placementCalculation.CalculatePlacementAsync(
            groupId,
            numberOfResults,
            cancellationToken
        );

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
