using WookiepediaStatusArticleData.Nominations.Awards;

namespace WookiepediaStatusArticleData.Services.Awards;

public interface IAwardGenerator
{
    Task<IList<Award>> GenerateAsync(AwardGenerationGroup generationGroup, CancellationToken cancellationToken);
}