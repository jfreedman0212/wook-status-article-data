using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Services.Awards;
using WookiepediaStatusArticleData.Services.Nominations;

namespace WookiepediaStatusArticleData.Controllers;

[AllowAnonymous]
[Route("/")]
public class HomeController(WookiepediaDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] int? awardId,
        [FromServices] TopAwardsLookup topAwardsLookup,
        [FromServices] TopProjectAwardsLookup topProjectAwardsLookup,
        CancellationToken cancellationToken
    )
    {
        var groups = await db.Set<AwardGenerationGroup>()
            .OrderByDescending(g => g.StartedAt)
            .ThenByDescending(g => g.EndedAt)
            .ThenBy(g => g.Name)
            .ToListAsync(cancellationToken);

        var selectedGroup =
            awardId != null
                ? groups.SingleOrDefault(it => it.Id == awardId.Value)
                : groups.FirstOrDefault();

        List<AwardHeadingViewModel>? awardHeadings = null;
        if (selectedGroup != null)
        {
            awardHeadings = await topAwardsLookup.LookupAsync(
                selectedGroup.Id,
                3,
                cancellationToken
            );
            
            var longestStatusArticle = await db.Set<Nomination>()
                .ForAwardCalculations(selectedGroup)
                .Where(it => it.EndWordCount != null)
                .OrderByDescending(it => it.EndWordCount)
                .FirstOrDefaultAsync(cancellationToken);
            
            var additionalAwardsHeadings = new AwardHeadingViewModel
            {
                Heading = "Additional Awards",
                Subheadings = []
            };

            if (longestStatusArticle != null)
            {
                additionalAwardsHeadings.Subheadings.Add(new SubheadingAwardViewModel
                {
                    Subheading = "Longest Status Article",
                    Awards = [
                        new AwardViewModel
                        {
                            Order = 0,
                            Heading = "Additional Awards",
                            Subheading = "Longest Status Article",
                            Type = longestStatusArticle.ArticleName,
                            Winners = [
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
                });
            }

            var projectAwardsSubheadings = await topProjectAwardsLookup.LookupAsync(
                selectedGroup.Id,
                10,
                cancellationToken
            );
            
            additionalAwardsHeadings.Subheadings.AddRange(projectAwardsSubheadings);
            
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
                    ComprehensiveCount = it.Count(nom => nom.Nomination.Type == NominationType.Comprehensive),
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

            additionalAwardsHeadings.Subheadings.Add(new SubheadingAwardViewModel
            {
                Subheading = "Most SA-Heavy Days",
                Awards =
                [
                    new AwardViewModel
                    {
                        Order = 0,
                        Heading = "Additional Awards",
                        Subheading = "Most SA-Heavy Days",
                        Type = "Overall",
                        Winners = [
                            new WinnerViewModel
                            {
                                Names = [topOverallDate!.Date.ToLongDateString()],
                                Count = topOverallDate.OverallCount
                            }
                        ]
                    },
                    new AwardViewModel
                    {
                        Order = 0,
                        Heading = "Additional Awards",
                        Subheading = "Most SA-Heavy Days",
                        Type = "Good",
                        Winners = [
                            new WinnerViewModel
                            {
                                Names = [topGoodDate!.Date.ToLongDateString()],
                                Count = topGoodDate.OverallCount
                            }
                        ]
                    },
                    new AwardViewModel
                    {
                        Order = 0,
                        Heading = "Additional Awards",
                        Subheading = "Most SA-Heavy Days",
                        Type = "Featured",
                        Winners = [
                            new WinnerViewModel
                            {
                                Names = [topFeaturedDate!.Date.ToLongDateString()],
                                Count = topFeaturedDate.OverallCount
                            }
                        ]
                    },
                    new AwardViewModel
                    {
                        Order = 0,
                        Heading = "Additional Awards",
                        Subheading = "Most SA-Heavy Days",
                        Type = "Comprehensive",
                        Winners = [
                            new WinnerViewModel
                            {
                                Names = [topComprehensiveDate!.Date.ToLongDateString()],
                                Count = topComprehensiveDate.OverallCount
                            }
                        ]
                    }
                ]
            });
            
            awardHeadings.Add(additionalAwardsHeadings);
        }

        return View(
            new HomePageViewModel
            {
                Groups = groups
                    .Select(g => new SelectListItem(g.Name, g.Id.ToString(), g.Id == awardId))
                    .ToList(),
                Selected =
                    awardHeadings != null && selectedGroup != null
                        ? new AwardGenerationGroupDetailViewModel
                        {
                            Id = selectedGroup.Id,
                            Name = selectedGroup.Name,
                            StartedAt = selectedGroup.StartedAt,
                            EndedAt = selectedGroup.EndedAt,
                            AwardHeadings = awardHeadings,
                        }
                        : null
            }
        );
    }

    [Route("/home/error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}

