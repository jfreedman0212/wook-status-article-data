using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Models.Nominators;

public class NominatorAttributeForm
{
    public int? Id { get; set; }
    public required NominatorAttributeType AttributeName { get; init; }
    public required DateOnly EffectiveAtDate { get; init; }
    public required TimeOnly EffectiveAtTime { get; init; }
    public DateOnly? EffectiveEndAtDate { get; init; }
    public TimeOnly? EffectiveEndAtTime { get; init; }
}