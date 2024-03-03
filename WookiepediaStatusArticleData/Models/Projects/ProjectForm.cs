using JetBrains.Annotations;

namespace WookiepediaStatusArticleData.Models.Projects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class ProjectForm
{
    public int? Id { get; set; }
    public required string Name { get; set; }
    public required DateOnly CreatedDate { get; set; }
    public required TimeOnly CreatedTime { get; set; }
}