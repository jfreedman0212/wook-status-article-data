namespace WookiepediaStatusArticleData.Models.Nominations;

public class NominationListViewModel
{
    public required NominationQuery Query { get; init; }
    public required IList<NominationViewModel> Page { get; init; }
    public required int TotalItems { get; init; }
}