using JetBrains.Annotations;

namespace WookiepediaStatusArticleData.Nominations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class Project
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
}