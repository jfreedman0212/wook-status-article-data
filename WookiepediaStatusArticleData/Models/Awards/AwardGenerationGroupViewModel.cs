namespace WookiepediaStatusArticleData.Models.Awards;

public class AwardGenerationGroupViewModel
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required DateTime StartedAt { get; set; }
    public required DateTime EndedAt { get; set; }
}