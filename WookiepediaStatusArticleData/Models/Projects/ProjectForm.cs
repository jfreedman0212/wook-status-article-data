using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Models.Projects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class ProjectForm
{
    public int? Id { get; set; }
    public required string Name { get; set; }
    public required ProjectType Type { get; set; }
    public required DateOnly CreatedDate { get; set; }
    public required TimeOnly CreatedTime { get; set; }
}