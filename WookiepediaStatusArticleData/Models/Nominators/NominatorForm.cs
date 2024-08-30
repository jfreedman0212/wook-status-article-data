using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Models.Nominators;

public class NominatorForm
{
    public int? Id { get; set; }
    public required string Name { get; init; }
    public IList<NominatorAttributeType> Attributes { get; set; } = [];
    public DateOnly? EffectiveAsOfDate { get; init; }
    public TimeOnly? EffectiveAsOfTime { get; init; }
}