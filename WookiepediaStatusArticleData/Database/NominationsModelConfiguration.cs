using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Nominations;
using WookiepediaStatusArticleData.Nominations.Projects;

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
            entity.Property(it => it.IsArchived).HasColumnName("is_archived");
        });
        
        modelBuilder.Entity<HistoricalProject>(entity =>
        {
            entity.ToTable("historical_projects");
            
            entity.Property(it => it.Id).HasColumnName("id");
            entity.Property(it => it.ProjectId).HasColumnName("project_id");
            entity.Property(it => it.ActionType).HasColumnName("action_type")
                .HasConversion(it => it.ToCode(), it => ProjectActionTypes.FromCode(it));
            entity.Property(it => it.Name).HasColumnName("name");
            entity.Property(it => it.OccurredAt).HasColumnName("occurred_at");

            entity.HasOne(it => it.Project)
                .WithMany(it => it.HistoricalValues)
                .HasForeignKey(it => it.ProjectId);
        });

        modelBuilder.Entity<Nominator>(entity =>
        {
            entity.ToTable("nominators");
            
            entity.Property(it => it.Id).HasColumnName("id");
            entity.Property(it => it.Name).HasColumnName("name");
        });

        modelBuilder.Entity<NominatorAttribute>(entity =>
        {
            entity.ToTable("nominator_attributes");

            entity.Property(it => it.Id).HasColumnName("id");
            entity.Property(it => it.NominatorId).HasColumnName("nominator_id");
            entity.Property(it => it.AttributeName).HasColumnName("attribute_name")
                .HasConversion(it => it.ToCode(), it => NominatorAttributeTypeExtensions.FromCode(it));
            entity.Property(it => it.EffectiveAt).HasColumnName("effective_at");
            entity.Property(it => it.EffectiveEndAt).HasColumnName("effective_end_at");
            
            entity.HasOne(it => it.Nominator)
                .WithMany(it => it.Attributes)
                .HasForeignKey(it => it.NominatorId);
        });
    }
}