namespace WookiepediaStatusArticleData.Models.Awards;

public class AwardViewModel
{
    public required int Id { get; init; }
    public required string Type { get; init; }
    public required string NominatorName { get; init; }
    public required int Count { get; init; }
}