namespace WookiepediaStatusArticleData.Models.Awards;

public class AwardGenerationGroupDetailViewModel
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required DateTime StartedAt { get; init; }
    public required DateTime EndedAt { get; init; }
    public required IList<AwardHeadingViewModel> AwardHeadings { get; init; }
}