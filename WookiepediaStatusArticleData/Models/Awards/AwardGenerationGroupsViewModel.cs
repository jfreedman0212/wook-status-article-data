using WookiepediaStatusArticleData.Nominations.Awards;

namespace WookiepediaStatusArticleData.Models.Awards;

public class AwardGenerationGroupsViewModel
{
    public required IList<AwardGenerationGroup> Groups { get; init; }
}