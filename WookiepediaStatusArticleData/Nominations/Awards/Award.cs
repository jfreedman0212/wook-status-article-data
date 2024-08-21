using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Nominations.Awards;

public class Award
{
    public int Id { get; set; }
    public int GenerationGroupId { get; set; }
    public AwardGenerationGroup? GenerationGroup { get; set; }
    public required int Type { get; set; }
    public int NominatorId { get; set; }
    public Nominator? Nominator { get; set; }
    public int Count { get; set; }
}
