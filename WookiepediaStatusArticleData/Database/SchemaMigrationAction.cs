using DbUp;
using SlashPineTech.Forestry.Lifecycle;

namespace WookiepediaStatusArticleData.Database;

public class SchemaMigrationAction(
    IConfiguration configuration,
    ILogger<MicrosoftLoggingUpgradeLog> logger
) : IStartupAction
{
    public Task OnStartupAsync(CancellationToken cancellationToken)
    {
        var connectionString = configuration["Database:ConnectionString"];

        var upgradeEngine = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(WookiepediaDbContext).Assembly, scriptName => scriptName.StartsWith("WookiepediaStatusArticleData.Database.Migrate."))
            .JournalToPostgresqlTable("public", "__schema_versions")
            .LogTo(new MicrosoftLoggingUpgradeLog(logger))
            .Build();

        upgradeEngine.PerformUpgrade();

        return Task.CompletedTask;
    }
}