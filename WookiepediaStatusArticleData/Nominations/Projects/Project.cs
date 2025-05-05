using JetBrains.Annotations;

namespace WookiepediaStatusArticleData.Nominations.Projects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class Project
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required ProjectType Type { get; set; }
    public required DateTime CreatedAt { get; set; }
    public bool IsArchived { get; set; }

    public IList<HistoricalProject>? HistoricalValues { get; set; }

    public void Archive()
    {
        IsArchived = true;
        HistoricalValues!.Add(new HistoricalProject
        {
            Name = Name,
            Type = Type,
            ActionType = ProjectActionType.Archive,
            OccurredAt = DateTime.UtcNow
        });
    }
}