using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Models.Nominators;

public class NominatorEditViewModel
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required IList<NominatorAttributeType> Attributes { get; init; } = [];
    public required IList<NominatorAttributeType> AllowedAttributes { get; init; }
}