using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Models.Nominators;

public class NominatorViewModel
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required IList<NominatorAttribute> Attributes { get; init; }
}