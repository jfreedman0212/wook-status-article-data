using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;

namespace WookiepediaStatusArticleData.Services.Awards.ProjectAwardCalculations;

public interface IProjectAwardCalculation
{
    string Name { get; }

    Task<IList<ProjectCountProjection>> GenerateAsync(
        AwardGenerationGroup awardGenerationGroup,
        CancellationToken cancellationToken
    );
}