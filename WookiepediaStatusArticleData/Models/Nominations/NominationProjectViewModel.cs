using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Models.Nominations;

public class NominationProjectViewModel
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required ProjectType Type { get; init; }
    public required bool IsArchived { get; init; }
}