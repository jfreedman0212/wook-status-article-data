namespace WookiepediaStatusArticleData.Nominations.Projects;

public enum ProjectType
{
    Category = 0,
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
    
    public static string GetDisplayClass(this ProjectType projectType)
    {
        return projectType switch
        {
            ProjectType.Category => "",
            ProjectType.IntellectualProperty => "italic",
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