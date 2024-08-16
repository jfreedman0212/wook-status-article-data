namespace WookiepediaStatusArticleData.Nominations.Awards;

public class AwardGenerationGroup
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required DateTime StartedAt { get; set; }
    public required DateTime EndedAt { get; set; }
    
    public IList<Award>? Awards { get; set; }
}