using Microsoft.EntityFrameworkCore;

namespace WookiepediaStatusArticleData.Database;

public interface IEntityModelConfiguration
{
    void OnModelCreating(ModelBuilder modelBuilder);
}