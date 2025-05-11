using Microsoft.AspNetCore.Mvc.Rendering;

namespace WookiepediaStatusArticleData.Models.Projects;

public class MergeProjectForm
{
    public IList<SelectListItem> AllProjects { get; set; } = [];

    public int ToProjectId { get; set; }
    public int FromProjectId { get; set; }
}