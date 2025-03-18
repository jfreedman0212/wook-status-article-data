using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;

namespace WookiepediaStatusArticleData.Services.Awards;

public class TopAwardsLookup(WookiepediaDbContext db)
{
    public async Task<List<AwardHeadingViewModel>> LookupAsync(
        AwardGenerationGroup group,
        CancellationToken cancellationToken
    )
    {
        var awards = await db.Set<Award>()
            .Where(it => it.GenerationGroupId == group.Id && it.Placement != AwardPlacement.DidNotPlace)
            .Include(it => it.Nominator)
            .ToListAsync(cancellationToken);

        return awards
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
                        Names = it.Select(x => x.Nominator!.Name).Order().ToList(),
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