using JetBrains.Annotations;

namespace WookiepediaStatusArticleData.Nominations.Projects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class HistoricalProject
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    public required ProjectActionType ActionType { get; set; }
    public required string Name { get; set; }
    public required ProjectType Type { get; set; }
    public required DateTime OccurredAt { get; set; }
}