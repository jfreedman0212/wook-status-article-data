using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Models.Projects;

public class ProjectsViewModel
{
    public required IList<Project> Projects { get; init; }
}