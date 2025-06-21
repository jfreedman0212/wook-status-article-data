using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Nominations.Awards;

/// <summary>
/// Defines filtering criteria for generating awards based on nomination data.
/// Replaces the hard-coded filtering logic in StaticNominatorAwardCalculation.
/// </summary>
public class AwardCriteria
{
    public int Id { get; set; }
    public int AwardTemplateId { get; set; }
    public AwardTemplate? AwardTemplate { get; set; }
    
    // Nomination Type filtering
    public NominationType? NominationType { get; set; }
    
    // Continuity filtering
    public Continuity? Continuity { get; set; }
    
    // Panelist filtering
    public AwardPanelistFilter? PanelistFilter { get; set; }
    
    // Project filtering
    public int? ProjectId { get; set; }
    public Project? Project { get; set; }
    public AwardProjectFilter? ProjectFilter { get; set; }
    
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Enum for filtering awards by panelist status.
/// Maps to the existing PanelistMode enum in NominationQueryBuilder.
/// </summary>
public enum AwardPanelistFilter
{
    All = 0,
    PanelistsOnly = 1,
    NonPanelistsOnly = 2
}

/// <summary>
/// Enum for filtering awards by project association.
/// </summary>
public enum AwardProjectFilter
{
    Any = 0,
    WithAnyProject = 1,
    WithNoProjects = 2,
    SpecificProject = 3
}
