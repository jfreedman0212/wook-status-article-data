using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Nominations;

namespace WookiepediaStatusArticleData.Database;

public class NominationsModelConfiguration : IEntityModelConfiguration
{
    public void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("projects");

            entity.Property(it => it.Id).HasColumnName("id");
            entity.Property(it => it.Name).HasColumnName("name");
            entity.Property(it => it.CreatedAt).HasColumnName("created_at");
        });
    }
}