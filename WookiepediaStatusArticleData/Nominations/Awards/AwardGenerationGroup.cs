namespace WookiepediaStatusArticleData.Nominations.Awards;

public class AwardGenerationGroup
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required DateTime StartedAt { get; set; }
    public required DateTime EndedAt { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
    
    public List<Award>? Awards { get; set; }
    public List<ProjectAward>? ProjectAwards { get; set; }
}