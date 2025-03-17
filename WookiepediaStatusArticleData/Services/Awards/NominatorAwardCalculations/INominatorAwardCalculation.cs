using WookiepediaStatusArticleData.Nominations.Awards;

namespace WookiepediaStatusArticleData.Services.Awards.NominatorAwardCalculations;

public interface INominatorAwardCalculation
{
    Task<IEnumerable<Award>> GenerateAsync(AwardGenerationGroup generationGroup, CancellationToken cancellationToken);
}