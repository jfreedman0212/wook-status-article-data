using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;

namespace WookiepediaStatusArticleData.Services.Awards.NominatorAwardCalculations;

/// <summary>
/// Generates awards based on configurable templates stored in the database.
/// This replaces the hard-coded logic in StaticNominatorAwardCalculation.
/// </summary>
public class ConfigurableNominatorAwardCalculation(WookiepediaDbContext db) : IConfigurableNominatorAwardCalculation
{
    public async Task<IEnumerable<Award>> GenerateAsync(AwardGenerationGroup generationGroup, CancellationToken cancellationToken)
    {
        var templates = await GetActiveAwardTemplatesAsync(cancellationToken);
        var awards = new List<Award>();

        foreach (var template in templates)
        {
            var templateAwards = await GenerateForTemplateAsync(template, generationGroup, cancellationToken);
            awards.AddRange(templateAwards);
        }

        return awards;
    }

    public async Task<IList<AwardTemplate>> GetActiveAwardTemplatesAsync(CancellationToken cancellationToken)
    {
        return await db.Set<AwardTemplate>()
            .Where(t => t.IsActive)
            .Include(t => t.Criteria!)
                .ThenInclude(c => c.Project)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Award>> GenerateForTemplateAsync(
        AwardTemplate template, 
        AwardGenerationGroup generationGroup, 
        CancellationToken cancellationToken)
    {
        // Build the query based on the template's criteria
        var queryBuilder = new NominationQueryBuilder(template.Heading, template.Subheading, template.Type, db);

        // Apply count mode
        queryBuilder = template.CountMode switch
        {
            AwardCountMode.NumberOfArticles => queryBuilder,
            AwardCountMode.NumberOfUniqueProjects => queryBuilder.WithCountMode(CountMode.NumberOfUniqueProjects),
            AwardCountMode.JocastaBotPoints => queryBuilder.WithCountMode(CountMode.JocastaBotPoints),
            _ => throw new ArgumentOutOfRangeException(nameof(template.CountMode), template.CountMode, null)
        };

        // Apply criteria from the template
        var builderWithCriteria = ApplyCriteriaToBuilder(queryBuilder, template.Criteria);

        return await builderWithCriteria.BuildAsync(generationGroup, cancellationToken);
    }

    private static IQueryBuilder ApplyCriteriaToBuilder(NominationQueryBuilder queryBuilder, ICollection<AwardCriteria>? criteria)
    {
        if (criteria == null || !criteria.Any())
        {
            return queryBuilder;
        }

        var builder = queryBuilder;

        foreach (var criterion in criteria)
        {
            // Apply nomination type filter
            if (criterion.NominationType.HasValue)
            {
                builder = builder.WithType(criterion.NominationType.Value);
            }

            // Apply continuity filter
            if (criterion.Continuity.HasValue)
            {
                builder = builder.WithContinuity(criterion.Continuity.Value);
            }

            // Apply project filters
            if (criterion.ProjectFilter.HasValue)
            {
                builder = criterion.ProjectFilter.Value switch
                {
                    AwardProjectFilter.WithAnyProject => builder.WithAnyWookieeProject(),
                    AwardProjectFilter.WithNoProjects => builder.WithNoWookieeProjects(),
                    AwardProjectFilter.SpecificProject when criterion.Project != null => builder.WithWookieeProject(criterion.Project),
                    AwardProjectFilter.Any => builder,
                    _ => builder
                };
            }
        }

        // Apply panelist filter (should be applied last as it returns a different builder type)
        var panelistCriterion = criteria.FirstOrDefault(c => c.PanelistFilter.HasValue);
        if (panelistCriterion?.PanelistFilter.HasValue == true)
        {
            return panelistCriterion.PanelistFilter.Value switch
            {
                AwardPanelistFilter.PanelistsOnly => builder.WithPanelistsOnly(),
                AwardPanelistFilter.NonPanelistsOnly => builder.WithNonPanelistsOnly(),
                AwardPanelistFilter.All => builder,
                _ => builder
            };
        }

        return builder;
    }
}
