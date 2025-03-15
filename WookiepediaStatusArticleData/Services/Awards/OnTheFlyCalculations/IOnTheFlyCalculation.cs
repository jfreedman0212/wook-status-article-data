using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;

namespace WookiepediaStatusArticleData.Services.Awards.OnTheFlyCalculations;

/// <summary>
/// Award calculation that's done on page load instead of calculated ahead of time.
/// This makes it simpler to implement for now, but keeps the controller nice and neat.
/// </summary>
public interface IOnTheFlyCalculation
{
    public Task<IList<SubheadingAwardViewModel>> CalculateAsync(
        AwardGenerationGroup selectedGroup,
        CancellationToken cancellationToken
    );
}