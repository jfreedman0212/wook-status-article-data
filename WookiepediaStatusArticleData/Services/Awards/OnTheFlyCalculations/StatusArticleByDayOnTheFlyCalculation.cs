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
            .WithOutcome(Outcome.Successful)
            .EndedWithinTimeframe(selectedGroup.StartedAt, selectedGroup.EndedAt)
            .Where(it => it.EndedAt != null)
            .GroupBy(it => it.EndedAt!.Value.Date);

        List<NominationType?> types = [null, NominationType.Good, NominationType.Featured, NominationType.Comprehensive];

        List<AwardViewModel> awards = [];

        for (int i = 0; i < types.Count; i++)
        {
            NominationType? type = types[i];

            var query = type == null
                ? baseQuery.Select(it => new 
                {
                    Date = it.Key,
                    Count = it.Count()
                })
                : baseQuery.Select(it => new
                {
                    Date = it.Key,
                    Count = it.Count(nom => nom.Type == type)
                });

            var maxCount = await query.Select(it => it.Count).MaxAsync(cancellationToken);
            var dates = await query
                .Where(it => it.Count == maxCount)
                .OrderBy(it => it.Date)
                .ToListAsync(cancellationToken);

            awards.Add(new AwardViewModel
            {
                Order = i,
                Heading = "Additional Awards",
                Subheading = "Most SA-Heavy Days",
                Type = type == null ? "Overall" : type.Value.GetDisplayName(),
                Winners = 
                [
                    new WinnerViewModel
                    {
                        Count = maxCount,
                        Names = dates.Select(it => it.Date.ToLongDateString()).ToList()
                    }
                ]
            });
        }

        return
        [
            new SubheadingAwardViewModel
            {
                Mode = TableMode.MostDaysWithArticles,
                Subheading = "Most SA-Heavy Days",
                Awards = awards
            }
        ];
    }
}