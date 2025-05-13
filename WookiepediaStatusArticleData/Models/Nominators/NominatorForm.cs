namespace WookiepediaStatusArticleData.Models.Nominators;

public class NominatorForm
{
    public int? Id { get; set; }
    public required string Name { get; init; }
    public required bool IsRedacted { get; init; }
    public IList<NominatorAttributeViewModel> Attributes { get; set; } = [];
}

