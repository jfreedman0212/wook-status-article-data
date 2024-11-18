using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Models.Nominators;

public class NominatorAttributeViewModel
{
    public int Id { get; init; }
    public required NominatorAttributeType AttributeName { get; init; }
    public required DateOnly EffectiveAt { get; init; }
    public DateOnly? EffectiveUntil { get; init; }
}
