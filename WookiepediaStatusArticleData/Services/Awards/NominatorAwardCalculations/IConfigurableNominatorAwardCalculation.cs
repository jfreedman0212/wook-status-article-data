using WookiepediaStatusArticleData.Nominations.Awards;

namespace WookiepediaStatusArticleData.Services.Awards.NominatorAwardCalculations;

/// <summary>
/// Award calculation that generates awards based on configurable templates from the database.
/// This replaces the hard-coded StaticNominatorAwardCalculation.
/// </summary>
public interface IConfigurableNominatorAwardCalculation : INominatorAwardCalculation
{
    /// <summary>
    /// Gets all active award templates from the database.
    /// </summary>
    Task<IList<AwardTemplate>> GetActiveAwardTemplatesAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Generates awards for a specific template.
    /// </summary>
    Task<IEnumerable<Award>> GenerateForTemplateAsync(
        AwardTemplate template, 
        AwardGenerationGroup generationGroup, 
        CancellationToken cancellationToken
    );
}
