using WookiepediaStatusArticleData.Nominations;

namespace WookiepediaStatusArticleData.Models.Projects;

public class ProjectsViewModel
{
    public required IList<Project> Projects { get; init; }
}