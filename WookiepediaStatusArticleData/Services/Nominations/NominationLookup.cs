using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Models.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominations;

namespace WookiepediaStatusArticleData.Services.Nominations;

public class NominationLookup(WookiepediaDbContext db)
{
    public async Task<NominationListViewModel> LookupAsync(NominationQuery query, CancellationToken cancellationToken)
    {
        var queryable = db.Set<Nomination>().Filter(query);
        var totalItems = await queryable.CountAsync(cancellationToken);
        var page = await queryable.Paginate(query, cancellationToken);

        return new NominationListViewModel
        {
            Page = page,
            TotalItems = totalItems
        };
    }
}