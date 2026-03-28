namespace WookiepediaStatusArticleData.Nominations.Awards;

/// <summary>
/// Defines a configurable award template that can be used to generate awards.
/// Replaces the hard-coded award definitions in StaticNominatorAwardCalculation.
/// </summary>
public class AwardTemplate
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Heading { get; set; }
    public required string Subheading { get; set; }
    public required string Type { get; set; }
    public required AwardCountMode CountMode { get; set; }
    public required bool IsActive { get; set; }
    public required int SortOrder { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }

    public List<AwardCriteria>? Criteria { get; set; }
}

/// <summary>
/// Enum representing the different ways to count nominations for awards.
/// Maps to the existing CountMode enum in NominationQueryBuilder.
/// </summary>
public enum AwardCountMode
{
    NumberOfArticles = 0,
    NumberOfUniqueProjects = 1,
    JocastaBotPoints = 2
}
