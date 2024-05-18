using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Models.Nominators;

public class NominatorAttributeViewModel
{
    public required int Id { get; init; }
    public required NominatorAttributeType AttributeName { get; init; }
    public required DateTime EffectiveAt { get; init; }
}