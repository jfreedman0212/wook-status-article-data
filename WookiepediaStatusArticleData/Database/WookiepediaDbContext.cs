using Microsoft.EntityFrameworkCore;

namespace WookiepediaStatusArticleData.Database;

public class WookiepediaDbContext(
    DbContextOptions<WookiepediaDbContext> options,
    IEnumerable<IEntityModelConfiguration> configurations
) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var cfg in configurations)
        {
            cfg.OnModelCreating(modelBuilder);
        }
    }
}