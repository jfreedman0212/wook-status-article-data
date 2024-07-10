using WookiepediaStatusArticleData.Nominations;

namespace WookiepediaStatusArticleData.Models.Projects;

public class ProjectViewModel
{
    public required IList<Project> Projects { get; init; }
}