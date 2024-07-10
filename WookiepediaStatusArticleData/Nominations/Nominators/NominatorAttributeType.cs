namespace WookiepediaStatusArticleData.Nominations.Nominators;

public enum NominatorAttributeType
{
    AcMember = 0,
    Inquisitor = 1,
    EduCorp = 2,
    Banned = 3
}

/// <summary>
/// Extensions for the <see cref="NominatorAttributeType"/> enumeration.
/// </summary>
public static class NominatorAttributeTypeExtensions
{
    /// <summary>
    /// Converts a string code to a <see cref="NominatorAttributeType"/> enumeration value.
    /// </summary>
    /// <param name="code">The string code to convert.</param>
    /// <returns>The converted <see cref="NominatorAttributeType"/> value.</returns>
    public static NominatorAttributeType FromCode(string code)
    {
        if (Enum.TryParse<NominatorAttributeType>(code, true, out var result))
        {
            return result;
        }
        
        throw new ArgumentException($"Cannot convert {code} to NominatorAttributeType");
    }

    /// <summary>
    /// Converts a <see cref="NominatorAttributeType"/> enumeration value to its corresponding string code.
    /// </summary>
    /// <param name="type">The <see cref="NominatorAttributeType"/> value to convert.</param>
    /// <returns>The string code representing the <see cref="NominatorAttributeType"/> value.</returns>
    public static string ToCode(this NominatorAttributeType type)
    {
        return type.ToString();
    }
    
    public static string ToDescription(this NominatorAttributeType type)
    {
        return type switch
        {
            NominatorAttributeType.AcMember => "AgriCorp",
            NominatorAttributeType.Inquisitor => "Inquisitor",
            NominatorAttributeType.EduCorp => "EduCorp",
            NominatorAttributeType.Banned => "Banned",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}