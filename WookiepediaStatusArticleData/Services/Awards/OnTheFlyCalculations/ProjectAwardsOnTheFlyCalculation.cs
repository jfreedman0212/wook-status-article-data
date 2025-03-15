using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;

namespace WookiepediaStatusArticleData.Services.Awards.OnTheFlyCalculations;

public class ProjectAwardsOnTheFlyCalculation(TopProjectAwardsLookup topProjectAwardsLookup) : IOnTheFlyCalculation
{
    public async Task<IList<SubheadingAwardViewModel>> CalculateAsync(
        AwardGenerationGroup selectedGroup,
        CancellationToken cancellationToken
    )
    {
        return await topProjectAwardsLookup.LookupAsync(
            selectedGroup.Id,
            10,
            cancellationToken
        );
    }
}