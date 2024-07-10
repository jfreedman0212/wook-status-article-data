using WookiepediaStatusArticleData.Nominations.Nominators;

namespace WookiepediaStatusArticleData.Models.Nominators;

public class NominatorsViewModel
{
    public required IList<Nominator> Nominators { get; init; }
}