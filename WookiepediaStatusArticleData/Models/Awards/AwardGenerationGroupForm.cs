namespace WookiepediaStatusArticleData.Models.Awards;

public class AwardGenerationGroupForm
{
    public required string Name { get; init; }
    public required DateOnly StartedAt { get; init; }
    public required DateOnly EndedAt { get; init; }
}