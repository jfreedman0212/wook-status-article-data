using WookiepediaStatusArticleData.Nominations;

namespace WookiepediaStatusArticleData.Models.Nominators;

public class NominatorsViewModel
{
    public required IList<Nominator> Nominators { get; init; }
}