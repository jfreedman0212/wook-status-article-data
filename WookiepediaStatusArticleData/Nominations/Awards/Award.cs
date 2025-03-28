using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Nominations.Awards;

public class Award
{
    public int Id { get; set; }
    public int GenerationGroupId { get; set; }
    public AwardGenerationGroup? GenerationGroup { get; set; }
    public required string Heading { get; set; }
    public required string Subheading { get; set; }
    public required string Type { get; set; }
    public int NominatorId { get; set; }
    public Nominator? Nominator { get; set; }
    public int Count { get; set; }
    public required AwardPlacement Placement { get; set; }
}
