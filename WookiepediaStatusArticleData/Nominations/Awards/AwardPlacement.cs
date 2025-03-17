namespace WookiepediaStatusArticleData.Nominations.Awards;

/// <summary>
/// Indicates whether a nominator placed (1st, 2nd, or 3rd) or didn't place for an award.
/// Codifying this in the database rather than on the fly makes calculating awards based on
/// this information easier.
/// </summary>
public enum AwardPlacement
{
    First = 0,
    Second,
    Third,
    DidNotPlace
}
