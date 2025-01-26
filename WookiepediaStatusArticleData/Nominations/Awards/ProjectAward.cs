using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Nominations.Awards;

/// <summary>
/// Similar to <see cref="Award"/> except it goes to a WookieeProject instead
/// of a Nominator.
/// </summary>
public class ProjectAward
{
    public int Id { get; set; }
    public int GenerationGroupId { get; set; }
    public AwardGenerationGroup? GenerationGroup { get; set; }
    public required string Heading { get; set; }
    public required string Type { get; set; }
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    public int Count { get; set; }
}