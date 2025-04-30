namespace WookiepediaStatusArticleData.Nominations.Nominations;

public enum Outcome
{
    Successful,
    Unsuccessful,
    Withdrawn,
    Other
}


public static class Outcomes
{
    public static string ToCode(this Outcome outcome)
    {
        return outcome switch
        {
            Outcome.Successful => "successful",
            Outcome.Unsuccessful => "unsuccessful",
            Outcome.Withdrawn => "withdrawn",
            Outcome.Other => "other",
            _ => throw new NotSupportedException($"No code mapped for {outcome}"),
        };
    }

    public static bool TryParseFromCode(string? code, out Outcome? result)
    {
        result = code?.ToLower() switch
        {
            "successful" => Outcome.Successful,
            "unsuccessful" => Outcome.Unsuccessful,
            "withdrawn" => Outcome.Withdrawn,
            "other" => Outcome.Other,
            _ => null
        };

        return result != null;
    }

    public static Outcome Parse(string code)
    {
        return TryParseFromCode(code, out var result)
            ? result!.Value
            : throw new NotSupportedException($"No code mapped for {code}");
    }
}
