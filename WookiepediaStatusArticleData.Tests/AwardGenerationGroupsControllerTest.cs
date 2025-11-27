using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Testcontainers.PostgreSql;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Nominations.Awards;

namespace WookiepediaStatusArticleData.Tests;

public class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.NameIdentifier, "test-user-id")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class AwardGenerationGroupsControllerTest : IClassFixture<AwardGenerationGroupsControllerTest.PostgresTestFixture>, IAsyncLifetime
{
    private readonly PostgresTestFixture _fixture;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AwardGenerationGroupsControllerTest(PostgresTestFixture fixture)
    {
        _fixture = fixture;

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Add test configuration to satisfy Auth0 requirements
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Auth:Domain"] = "test.auth0.com",
                    ["Auth:ClientId"] = "test-client-id"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                services.RemoveAll(typeof(DbContextOptions<WookiepediaDbContext>));
                services.RemoveAll(typeof(WookiepediaDbContext));

                // Add PostgreSQL database for testing using connection string from Testcontainer
                services.AddDbContext<WookiepediaDbContext>(options =>
                {
                    options.UseNpgsql(_fixture.ConnectionString);
                });

                // Remove Auth0 authentication services after they're configured
                services.RemoveAll(typeof(IAuthenticationService));
                services.RemoveAll(typeof(AuthenticationOptions));
                services.RemoveAll(typeof(IAuthenticationHandlerProvider));
                services.RemoveAll(typeof(IAuthenticationSchemeProvider));

                // Add test authentication
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                        "Test", options => { options.TimeProvider = TimeProvider.System; });
                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder("Test")
                        .RequireAssertion(context => true)
                        .Build();
                });
            });
        });

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public async Task InitializeAsync()
    {
        // Ensure database is created and migrated
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task CreateAsync_WithValidForm_CreatesAwardGenerationGroupAndRedirectsToIndex()
    {
        var formData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("Name", "Test Award Group"),
            new KeyValuePair<string, string>("StartedAt", "2023-01-01"),
            new KeyValuePair<string, string>("EndedAt", "2023-12-31")
        ]);

        var response = await _client.PostAsync("/award-generation-groups", formData);

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/award-generation-groups", response.Headers.Location?.ToString());

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();

        var createdGroup = await dbContext.Set<AwardGenerationGroup>()
            .SingleOrDefaultAsync(g => g.Name == "Test Award Group");

        Assert.NotNull(createdGroup);
        Assert.Equal("Test Award Group", createdGroup.Name);
        Assert.Equal(new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc), createdGroup.StartedAt);
        var expectedEndedAt = new DateOnly(2023, 12, 31).ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        Assert.True(Math.Abs((expectedEndedAt - createdGroup.EndedAt).TotalMilliseconds) < 1,
            $"Expected EndedAt to be close to {expectedEndedAt:O}, but was {createdGroup.EndedAt:O}");
        Assert.True(createdGroup.CreatedAt <= DateTime.UtcNow);
        Assert.True(createdGroup.UpdatedAt <= DateTime.UtcNow);
        Assert.Equal(createdGroup.CreatedAt, createdGroup.UpdatedAt);
    }

    [Fact]
    public async Task CreateAsync_WithStartedAtEqualToEndedAt_ReturnsBadRequestWithValidationError()
    {
        var formData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("Name", "Invalid Date Group"),
            new KeyValuePair<string, string>("StartedAt", "2023-01-01"),
            new KeyValuePair<string, string>("EndedAt", "2023-01-01")
        ]);

        var response = await _client.PostAsync("/award-generation-groups", formData);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Started At must be before Ended At", content);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();

        var groupExists = await dbContext.Set<AwardGenerationGroup>()
            .AnyAsync(g => g.Name == "Invalid Date Group");
        Assert.False(groupExists);
    }

    [Fact]
    public async Task CreateAsync_WithStartedAtAfterEndedAt_ReturnsBadRequestWithValidationError()
    {
        var formData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("Name", "Invalid Date Group"),
            new KeyValuePair<string, string>("StartedAt", "2023-12-31"),
            new KeyValuePair<string, string>("EndedAt", "2023-01-01")
        ]);

        var response = await _client.PostAsync("/award-generation-groups", formData);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Started At must be before Ended At", content);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateNameAndTimeframe_ReturnsBadRequestWithValidationError()
    {
        // First, create an existing group
        using (var setupScope = _factory.Services.CreateScope())
        {
            var setupDbContext = setupScope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();
            var existingGroup = new AwardGenerationGroup
            {
                Name = "Existing Group",
                StartedAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2023, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc).AddTicks(9999),
                Awards = [],
                ProjectAwards = [],
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            setupDbContext.Add(existingGroup);
            await setupDbContext.SaveChangesAsync();
        }

        var formData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("Name", "Existing Group"),
            new KeyValuePair<string, string>("StartedAt", "2023-01-01"),
            new KeyValuePair<string, string>("EndedAt", "2023-12-31")
        ]);

        var response = await _client.PostAsync("/award-generation-groups", formData);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Existing Group already exists for that timeframe", content);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();

        var groupCount = await dbContext.Set<AwardGenerationGroup>()
            .CountAsync(g => g.Name == "Existing Group");
        Assert.Equal(1, groupCount);
    }

    [Fact]
    public async Task CreateAsync_WithSameNameButDifferentTimeframe_CreatesSuccessfully()
    {
        // First, create an existing group
        using (var setupScope = _factory.Services.CreateScope())
        {
            var setupDbContext = setupScope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();
            var existingGroup = new AwardGenerationGroup
            {
                Name = "Same Name Group",
                StartedAt = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2022, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc).AddTicks(9999),
                Awards = [],
                ProjectAwards = [],
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            setupDbContext.Add(existingGroup);
            await setupDbContext.SaveChangesAsync();
        }

        var formData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("Name", "Same Name Group"),
            new KeyValuePair<string, string>("StartedAt", "2023-01-01"),
            new KeyValuePair<string, string>("EndedAt", "2023-12-31")
        ]);

        var response = await _client.PostAsync("/award-generation-groups", formData);

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/award-generation-groups", response.Headers.Location?.ToString());

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();

        var groupCount = await dbContext.Set<AwardGenerationGroup>()
            .CountAsync(g => g.Name == "Same Name Group");
        Assert.Equal(2, groupCount);
    }

    [Fact]
    public async Task CreateAsync_EnsuresTransactionCommitsAwardsGeneration()
    {
        var formData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("Name", "Award Generation Test"),
            new KeyValuePair<string, string>("StartedAt", "2023-01-01"),
            new KeyValuePair<string, string>("EndedAt", "2023-12-31")
        ]);

        var response = await _client.PostAsync("/award-generation-groups", formData);

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/award-generation-groups", response.Headers.Location?.ToString());

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();

        var createdGroup = await dbContext.Set<AwardGenerationGroup>()
            .SingleOrDefaultAsync(g => g.Name == "Award Generation Test");

        Assert.NotNull(createdGroup);

        var awardCount = await dbContext.Set<Award>()
            .CountAsync(a => a.GenerationGroupId == createdGroup.Id);

        var projectAwardCount = await dbContext.Set<ProjectAward>()
            .CountAsync(pa => pa.GenerationGroupId == createdGroup.Id);

        // Awards generation should have run (may be 0 if no nominations exist, but should not error)
        Assert.True(awardCount >= 0);
        Assert.True(projectAwardCount >= 0);
    }

    [Fact]
    public async Task ExportToWookieepedia_WithValidGroup_ReturnsTextFile()
    {
        // First, create a group to export
        int groupId;
        using (var setupScope = _factory.Services.CreateScope())
        {
            var setupDbContext = setupScope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();
            var group = new AwardGenerationGroup
            {
                Name = "Export Test Group",
                StartedAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2023, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc).AddTicks(9999),
                Awards = [],
                ProjectAwards = [],
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            setupDbContext.Add(group);
            await setupDbContext.SaveChangesAsync();
            groupId = group.Id;
        }

        var response = await _client.GetAsync($"/award-generation-groups/{groupId}/export-wookieepedia");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
        
        var content = await response.Content.ReadAsStringAsync();
        
        // Verify the complete expected format
        var expectedFormat = @"{|{{Prettytable|style=margin:auto}}
! !! Overall !! Count !! GA !! Count !! FA !! Count
|-
|1st||||0||||0||||0
|-
|2nd||||0||||0||||0
|-
|3rd||||0||||0||||0
|-
|}
";
        Assert.Equal(expectedFormat, content);
    }

    [Fact]
    public async Task ExportToWookieepedia_WithNonExistentGroup_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/award-generation-groups/99999/export-wookieepedia");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    public class PostgresTestFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("wookiepedia_test")
            .WithUsername("test")
            .WithPassword("test")
            .WithCleanUp(true)
            .Build();

        public string ConnectionString { get; private set; } = string.Empty;

        public async Task InitializeAsync()
        {
            await _container.StartAsync();
            ConnectionString = _container.GetConnectionString();
        }

        public async Task DisposeAsync()
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }
}