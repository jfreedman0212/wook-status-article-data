using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using SlashPineTech.Forestry.ServiceModules;

namespace WookiepediaStatusArticleData.Database;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class DatabaseModule : IServiceModule
{
    public required string ConnectionString { get; init; }
    public bool EnableSensitiveDataLogging { get; init; }

    public void Configure(IServiceCollection services, IServiceConfigurationContext ctx)
    {
        services.AddDbContext<WookiepediaDbContext>(options =>
        {
            options.UseNpgsql(ConnectionString)
                .EnableSensitiveDataLogging(EnableSensitiveDataLogging);
        });

        services.AddHealthChecks().AddDbContextCheck<WookiepediaDbContext>();

        services.AddTransient<IEntityModelConfiguration, NominationsModelConfiguration>();
    }
}