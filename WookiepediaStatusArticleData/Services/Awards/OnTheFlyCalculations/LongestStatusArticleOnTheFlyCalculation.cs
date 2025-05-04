using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Services.Nominations;

namespace WookiepediaStatusArticleData.Services.Awards.OnTheFlyCalculations;

public class LongestStatusArticleOnTheFlyCalculation(WookiepediaDbContext db) : IOnTheFlyCalculation
{
    public async Task<IList<SubheadingAwardViewModel>> CalculateAsync(
        AwardGenerationGroup selectedGroup,
        CancellationToken cancellationToken
    )
    {
        var longestStatusArticle = await db.Set<Nomination>()
            .ForAwardCalculations(selectedGroup)
            .Where(it => it.EndWordCount != null)
            .OrderByDescending(it => it.EndWordCount)
            .FirstOrDefaultAsync(cancellationToken);

        if (longestStatusArticle == null) return [];

        return [
            new SubheadingAwardViewModel
            {
                Mode = TableMode.Default,
                Subheading = "Longest Status Article",
                Awards =
                [
                    new AwardViewModel
                    {
                        Order = 0,
                        Heading = "Additional Awards",
                        Subheading = "Longest Status Article",
                        Type = longestStatusArticle.ArticleName,
                        Winners =
                        [
                            new WinnerViewModel
                            {
                                Count = longestStatusArticle.EndWordCount!.Value,
                                Names = longestStatusArticle.Nominators!
                                    .Select(it => it.Name)
                                    .Order()
                                    .ToList()
                            }
                        ]
                    }
                ]
            }
        ];
    }
}