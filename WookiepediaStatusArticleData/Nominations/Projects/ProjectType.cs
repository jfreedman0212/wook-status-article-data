using System.ComponentModel.DataAnnotations;

namespace WookiepediaStatusArticleData.Nominations.Projects;

public enum ProjectType
{
    [Display(Name = "Category")]
    Category = 0,
    [Display(Name = "Intellectual Property")]
    IntellectualProperty = 1
}

public static class ProjectTypes
{
    public static string ToCode(this ProjectType projectType)
    {
        return projectType switch
        {
            ProjectType.Category => "Category",
            ProjectType.IntellectualProperty => "IntellectualProperty",
            _ => throw new ArgumentOutOfRangeException(nameof(projectType), projectType, null)
        };
    }

    public static string ToDescription(this ProjectType projectType)
    {
        return projectType switch
        {
            ProjectType.Category => "Category",
            ProjectType.IntellectualProperty => "Intellectual Property",
            _ => throw new ArgumentOutOfRangeException(nameof(projectType), projectType, null)
        };
    }

    public static ProjectType FromCode(string code)
    {
        return code switch
        {
            "Category" => ProjectType.Category,
            "IntellectualProperty" => ProjectType.IntellectualProperty,
            _ => throw new ArgumentOutOfRangeException(nameof(code), code,
                "Code does not correspond to a known ProjectType.")
        };
    }
}