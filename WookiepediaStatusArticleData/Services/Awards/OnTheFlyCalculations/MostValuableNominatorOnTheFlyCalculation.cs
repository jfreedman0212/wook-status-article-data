using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Awards;
using WookiepediaStatusArticleData.Nominations.Awards;

namespace WookiepediaStatusArticleData.Services.Awards.OnTheFlyCalculations;

public class MostValuableNominatorOnTheFlyCalculation(WookiepediaDbContext db) : IOnTheFlyCalculation
{
    public async Task<IList<SubheadingAwardViewModel>> CalculateAsync(
        AwardGenerationGroup selectedGroup,
        CancellationToken cancellationToken
    )
    {
        // TODO: needs to dedupe by points

        var panelistWinners = await db.Set<Award>()
            .Where(it => it.GenerationGroupId == selectedGroup.Id)
            .WithoutBannedNominators(selectedGroup.CreatedAt)
            .WithPanelistsOnly(selectedGroup.StartedAt, selectedGroup.EndedAt)
            .MostValuableNominatorPoints()
            .OrderByDescending(it => it.Points)
            .Take(1)
            .ToListAsync(cancellationToken);

        var nonPanelistWinners = await db.Set<Award>()
            .Where(it => it.GenerationGroupId == selectedGroup.Id)
            .WithoutBannedNominators(selectedGroup.CreatedAt)
            .WithNonPanelistsOnly(selectedGroup.StartedAt, selectedGroup.EndedAt)
            .MostValuableNominatorPoints()
            .OrderByDescending(it => it.Points)
            .Take(1)
            .ToListAsync(cancellationToken);

        return [
            new SubheadingAwardViewModel
            {
                Subheading = "Most Valuable Nominators",
                Awards = [
                    new AwardViewModel
                    {
                        Order = 0,
                        Type = "Panelists",
                        Winners = panelistWinners
                            .Select(winner => new WinnerViewModel
                            {
                                Names = [winner.Nominator.Name],
                                Count = winner.Points
                            })
                            .ToList(),
                        Heading = "Additional Awards",
                        Subheading = "Most Valuable Nominators",
                    },
                    new AwardViewModel
                    {
                        Order = 1,
                        Type = "Non-Panelists",
                        Winners = nonPanelistWinners
                            .Select(winner => new WinnerViewModel
                            {
                                Names = [winner.Nominator.Name],
                                Count = winner.Points
                            })
                            .ToList(),
                        Heading = "Additional Awards",
                        Subheading = "Most Valuable Nominators",
                    }
                ]
            }
        ];
    }
}