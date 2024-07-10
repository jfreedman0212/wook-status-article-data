namespace WookiepediaStatusArticleData.Nominations.Nominations;

public enum Continuity
{
    Legends = 1,
    OutOfUniverse = 2,
    Canon = 4,
    NonCanon = 8,
    NonLegends = 16
}


public static class ContinuityExtensions 
{
    public static bool TryParseFromCode(string? code, out Continuity? continuity)
    {
        continuity = code?.ToLowerInvariant() switch
        {
            "legends" => Continuity.Legends,
            "oou" => Continuity.OutOfUniverse,
            "canon" => Continuity.Canon,
            "non-canon" => Continuity.NonCanon,
            "non-legends" => Continuity.NonLegends,
            _ => null
        };
        
        return continuity != null;
    }

    public static IList<Continuity> FromBitmask(int bitmask)
    {
        return Enum.GetValues<Continuity>().Where(continuity => (bitmask & (int)continuity) > 0).ToList();
    }

    public static int ToBitmask(this IEnumerable<Continuity> continuities)
    {
        return continuities.Aggregate(0, (currentMask, continuity) => currentMask | (int)continuity);
    }
}