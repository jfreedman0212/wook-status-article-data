using System.ComponentModel.DataAnnotations;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Models.Awards;

public class AwardTemplatesViewModel
{
    public required IList<AwardTemplate> Templates { get; init; }
}

public class AwardTemplateFormViewModel
{
    public AwardTemplateForm? Form { get; init; }
    public required IList<Project> Projects { get; init; }
}

public class AwardTemplateForm
{
    [Required]
    [StringLength(200)]
    public required string Name { get; init; }

    [Required]
    [StringLength(1000)]
    public required string Description { get; init; }

    [Required]
    [StringLength(100)]
    public required string Heading { get; init; }

    [Required]
    [StringLength(100)]
    public required string Subheading { get; init; }

    [Required]
    [StringLength(100)]
    public required string Type { get; init; }

    [Required]
    public required AwardCountMode CountMode { get; init; }

    public required bool IsActive { get; init; } = true;

    [Range(0, int.MaxValue)]
    public required int SortOrder { get; init; } = 0;

    // Criteria fields
    public NominationType? NominationType { get; init; }
    public Continuity? Continuity { get; init; }
    public AwardPanelistFilter? PanelistFilter { get; init; }
    public AwardProjectFilter? ProjectFilter { get; init; }
    public int? ProjectId { get; init; }
}
