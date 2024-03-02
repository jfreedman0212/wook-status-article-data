using JetBrains.Annotations;

namespace WookiepediaStatusArticleData.Models.Projects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class ProjectForm
{
    public required string Name { get; set; }
}