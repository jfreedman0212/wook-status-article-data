using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Models.Projects;

public class ProjectViewModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required ProjectType Type { get; set; }
    public required DateTime CreatedAt { get; set; }
}