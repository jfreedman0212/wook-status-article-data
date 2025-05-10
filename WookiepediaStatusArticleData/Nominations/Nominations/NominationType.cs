using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace WookiepediaStatusArticleData.Nominations.Nominations;

public enum NominationType
{
    [Display(Name = "Featured")]
    Featured,

    [Display(Name = "Good")]
    Good,

    [Display(Name = "Comprehensive")]
    Comprehensive
}


public static class NominationTypes
{
    public static string ToCode(this NominationType type)
    {
        return type switch
        {
            NominationType.Featured => "FAN",
            NominationType.Good => "GAN",
            NominationType.Comprehensive => "CAN",
            _ => throw new NotSupportedException($"No code mapped for {type}"),
        };
    }

    public static bool TryParseFromCode(string? code, out NominationType? result)
    {
        result = code?.ToUpperInvariant() switch
        {
            "FAN" => NominationType.Featured,
            "GAN" => NominationType.Good,
            "CAN" => NominationType.Comprehensive,
            _ => null
        };

        return result != null;
    }

    public static NominationType Parse(string? code)
    {
        return TryParseFromCode(code, out var result)
            ? result!.Value
            : throw new NotSupportedException($"No code mapped for {code}");
    }

    public static string GetDisplayName(this NominationType type) => type.GetType()
            .GetMember(type.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>()?
            .Name
            ?? type.ToString();
}
