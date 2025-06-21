using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Nominations.Awards;
using WookiepediaStatusArticleData.Nominations.Nominations;
using WookiepediaStatusArticleData.Nominations.Nominators;
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
            entity.Property(it => it.Type).HasColumnName("type")
                .HasConversion(it => it.ToCode(), it => ProjectTypes.FromCode(it));
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
            entity.Property(it => it.Type).HasColumnName("type")
                .HasConversion(it => it.ToCode(), it => ProjectTypes.FromCode(it));
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
            entity.Property(it => it.IsRedacted).HasColumnName("is_redacted");
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

        modelBuilder.Entity<Nomination>(entity =>
        {
            entity.ToTable("nominations");

            entity.Property(it => it.Id).HasColumnName("id");
            entity.Property(it => it.ArticleName).HasColumnName("article_name");
            entity.Property(it => it.Continuities).HasColumnName("continuities")
                .HasConversion(
                    continuities => continuities.ToBitmask(),
                    bitmask => ContinuityExtensions.FromBitmask(bitmask)
                )
                .HasDefaultValue(new List<Continuity>());
            entity.Property(it => it.Type).HasColumnName("type")
                .HasConversion(
                    reason => reason.ToCode(),
                    code => NominationTypes.Parse(code)
                );
            entity.Property(it => it.Outcome).HasColumnName("outcome")
                .HasConversion(
                    reason => reason.ToCode(),
                    code => Outcomes.Parse(code)
                );
            entity.Property(it => it.StartedAt).HasColumnName("started_at");
            entity.Property(it => it.EndedAt).HasColumnName("ended_at");
            entity.Property(it => it.StartWordCount).HasColumnName("start_word_count");
            entity.Property(it => it.EndWordCount).HasColumnName("end_word_count");

            entity.HasMany(it => it.Nominators)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "nomination_nominators",
                    join => join
                        .HasOne<Nominator>()
                        .WithMany()
                        .HasForeignKey("nominator_id")
                        .OnDelete(DeleteBehavior.Cascade),
                    join => join
                        .HasOne<Nomination>()
                        .WithMany()
                        .HasForeignKey("nomination_id")
                        .OnDelete(DeleteBehavior.ClientCascade)
                );

            entity.HasMany(it => it.Projects)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "nomination_projects",
                    join => join
                        .HasOne<Project>()
                        .WithMany()
                        .HasForeignKey("project_id")
                        .OnDelete(DeleteBehavior.Cascade),
                    join => join
                        .HasOne<Nomination>()
                        .WithMany()
                        .HasForeignKey("nomination_id")
                        .OnDelete(DeleteBehavior.ClientCascade)
                );
        });

        modelBuilder.Entity<AwardGenerationGroup>(entity =>
        {
            entity.ToTable("award_generation_groups");

            entity.Property(it => it.Id).HasColumnName("id");
            entity.Property(it => it.Name).HasColumnName("name");
            entity.Property(it => it.StartedAt).HasColumnName("started_at");
            entity.Property(it => it.EndedAt).HasColumnName("ended_at");
            entity.Property(it => it.CreatedAt).HasColumnName("created_at");
            entity.Property(it => it.UpdatedAt).HasColumnName("updated_at");

            entity.HasMany(it => it.Awards)
                .WithOne(it => it.GenerationGroup)
                .HasForeignKey(it => it.GenerationGroupId);

            entity.HasMany(it => it.ProjectAwards)
                .WithOne(it => it.GenerationGroup)
                .HasForeignKey(it => it.GenerationGroupId);
        });

        modelBuilder.Entity<Award>(entity =>
        {
            entity.ToTable("awards");

            entity.Property(it => it.Id).HasColumnName("id");
            entity.Property(it => it.GenerationGroupId).HasColumnName("generation_group_id");
            entity.Property(it => it.Heading).HasColumnName("heading");
            entity.Property(it => it.Subheading).HasColumnName("subheading");
            entity.Property(it => it.Type).HasColumnName("type");
            entity.Property(it => it.NominatorId).HasColumnName("nominator_id");
            entity.Property(it => it.Count).HasColumnName("count");
            entity.Property(it => it.Placement).HasColumnName("placement");

            entity.HasOne(it => it.Nominator)
                .WithMany()
                .HasForeignKey(it => it.NominatorId);
        });

        modelBuilder.Entity<ProjectAward>(entity =>
        {
            entity.ToTable("project_awards");

            entity.Property(it => it.Id).HasColumnName("id");
            entity.Property(it => it.GenerationGroupId).HasColumnName("generation_group_id");
            entity.Property(it => it.Heading).HasColumnName("heading");
            entity.Property(it => it.Type).HasColumnName("type");
            entity.Property(it => it.ProjectId).HasColumnName("project_id");
            entity.Property(it => it.Count).HasColumnName("count");

            entity.HasOne(it => it.Project)
                .WithMany()
                .HasForeignKey(it => it.ProjectId);
        });

        modelBuilder.Entity<AwardTemplate>(entity =>
        {
            entity.ToTable("award_templates");

            entity.Property(it => it.Id).HasColumnName("id");
            entity.Property(it => it.Name).HasColumnName("name");
            entity.Property(it => it.Description).HasColumnName("description");
            entity.Property(it => it.Heading).HasColumnName("heading");
            entity.Property(it => it.Subheading).HasColumnName("subheading");
            entity.Property(it => it.Type).HasColumnName("type");
            entity.Property(it => it.CountMode).HasColumnName("count_mode")
                .HasConversion<int>();
            entity.Property(it => it.IsActive).HasColumnName("is_active");
            entity.Property(it => it.SortOrder).HasColumnName("sort_order");
            entity.Property(it => it.CreatedAt).HasColumnName("created_at");
            entity.Property(it => it.UpdatedAt).HasColumnName("updated_at");

            entity.HasMany(it => it.Criteria)
                .WithOne(it => it.AwardTemplate)
                .HasForeignKey(it => it.AwardTemplateId);
        });

        modelBuilder.Entity<AwardCriteria>(entity =>
        {
            entity.ToTable("award_criteria");

            entity.Property(it => it.Id).HasColumnName("id");
            entity.Property(it => it.AwardTemplateId).HasColumnName("award_template_id");
            entity.Property(it => it.NominationType).HasColumnName("nomination_type")
                .HasConversion(
                    type => type.HasValue ? type.Value.ToCode() : null,
                    code => !string.IsNullOrEmpty(code) ? NominationTypes.Parse(code) : null
                );
            entity.Property(it => it.Continuity).HasColumnName("continuity")
                .HasConversion<int?>();
            entity.Property(it => it.PanelistFilter).HasColumnName("panelist_filter")
                .HasConversion<int?>();
            entity.Property(it => it.ProjectId).HasColumnName("project_id");
            entity.Property(it => it.ProjectFilter).HasColumnName("project_filter")
                .HasConversion<int?>();
            entity.Property(it => it.CreatedAt).HasColumnName("created_at");
            entity.Property(it => it.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(it => it.Project)
                .WithMany()
                .HasForeignKey(it => it.ProjectId);
        });
    }
}