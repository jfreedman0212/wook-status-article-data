namespace WookiepediaStatusArticleData.Nominations.Projects;

public enum ProjectActionType
{
    Create,
    Update,
    Archive
}

public static class ProjectActionTypes 
{
    public static string ToCode(this ProjectActionType actionType) 
    {
        return actionType.ToString();
    }

    public static ProjectActionType FromCode(string code) 
    {
        if (Enum.TryParse(code, out ProjectActionType result))
        {
            return result;
        }
        
        throw new ArgumentException($"Invalid project action code: {code}");
    }
}