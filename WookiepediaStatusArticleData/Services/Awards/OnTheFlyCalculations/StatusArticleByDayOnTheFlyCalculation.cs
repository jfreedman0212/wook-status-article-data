using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Services.Nominations;

namespace WookiepediaStatusArticleData.Services.Awards.OnTheFlyCalculations;

public class StatusArticleByDayOnTheFlyCalculation(WookiepediaDbContext db) : IOnTheFlyCalculation
{
    public async Task<IList<SubheadingAwardViewModel>> CalculateAsync(
        AwardGenerationGroup selectedGroup,
        CancellationToken cancellationToken
    )
    {
        var baseQuery = db.Set<Nomination>()
            .ForAwardCalculations(selectedGroup)
            .Where(it => it.EndedAt != null)
            .Select(it => new
            {
                Nomination = it,
                EndedAtDate = it.EndedAt!.Value.Date
            })
            .GroupBy(it => it.EndedAtDate)
            .Select(it => new
            {
                Date = it.Key,
                OverallCount = it.Count(),
                GoodCount = it.Count(nom => nom.Nomination.Type == NominationType.Good),
                FeaturedCount = it.Count(nom => nom.Nomination.Type == NominationType.Featured),
                ComprehensiveCount = it.Count(nom => nom.Nomination.Type == NominationType.Comprehensive)
            });

        var topOverallDate = await baseQuery
            .OrderByDescending(it => it.OverallCount)
            .FirstOrDefaultAsync(cancellationToken);
        var topGoodDate = await baseQuery
            .OrderByDescending(it => it.GoodCount)
            .FirstOrDefaultAsync(cancellationToken);
        var topFeaturedDate = await baseQuery
            .OrderByDescending(it => it.FeaturedCount)
            .FirstOrDefaultAsync(cancellationToken);
        var topComprehensiveDate = await baseQuery
            .OrderByDescending(it => it.ComprehensiveCount)
            .FirstOrDefaultAsync(cancellationToken);

        return
        [
            new SubheadingAwardViewModel
            {
                Mode = TableMode.MostDaysWithArticles,
                Subheading = "Most SA-Heavy Days",
                Awards =
                [
                    new AwardViewModel
                    {
                        Order = 0,
                        Heading = "Additional Awards",
                        Subheading = "Most SA-Heavy Days",
                        Type = "Overall",
                        Winners =
                        [
                            new WinnerViewModel
                            {
                                Names = [topOverallDate!.Date.ToLongDateString()],
                                Count = topOverallDate.OverallCount
                            }
                        ]
                    },
                    new AwardViewModel
                    {
                        Order = 1,
                        Heading = "Additional Awards",
                        Subheading = "Most SA-Heavy Days",
                        Type = "Good",
                        Winners =
                        [
                            new WinnerViewModel
                            {
                                Names = [topGoodDate!.Date.ToLongDateString()],
                                Count = topGoodDate.OverallCount
                            }
                        ]
                    },
                    new AwardViewModel
                    {
                        Order = 2,
                        Heading = "Additional Awards",
                        Subheading = "Most SA-Heavy Days",
                        Type = "Featured",
                        Winners =
                        [
                            new WinnerViewModel
                            {
                                Names = [topFeaturedDate!.Date.ToLongDateString()],
                                Count = topFeaturedDate.OverallCount
                            }
                        ]
                    },
                    new AwardViewModel
                    {
                        Order = 3,
                        Heading = "Additional Awards",
                        Subheading = "Most SA-Heavy Days",
                        Type = "Comprehensive",
                        Winners =
                        [
                            new WinnerViewModel
                            {
                                Names = [topComprehensiveDate!.Date.ToLongDateString()],
                                Count = topComprehensiveDate.OverallCount
                            }
                        ]
                    }
                ]
            }
        ];
    }
}