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
using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Tests;

public class ProjectsControllerTest : IClassFixture<ProjectsControllerTest.PostgresTestFixture>, IAsyncLifetime
{
    private readonly PostgresTestFixture _fixture;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProjectsControllerTest(PostgresTestFixture fixture)
    {
        _fixture = fixture;

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Auth:Domain"] = "test.auth0.com",
                    ["Auth:ClientId"] = "test-client-id"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<WookiepediaDbContext>));
                services.RemoveAll(typeof(WookiepediaDbContext));

                services.AddDbContext<WookiepediaDbContext>(options =>
                {
                    options.UseNpgsql(_fixture.ConnectionString);
                });

                services.RemoveAll(typeof(IAuthenticationService));
                services.RemoveAll(typeof(AuthenticationOptions));
                services.RemoveAll(typeof(IAuthenticationHandlerProvider));
                services.RemoveAll(typeof(IAuthenticationSchemeProvider));

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
    public async Task Add_WithValidUniqueCode_CreatesProjectSuccessfully()
    {
        var formData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("Name", "Test Project"),
            new KeyValuePair<string, string>("Code", "TEST-001"),
            new KeyValuePair<string, string>("Type", "0"),
            new KeyValuePair<string, string>("CreatedDate", "2023-01-01"),
            new KeyValuePair<string, string>("CreatedTime", "12:00")
        ]);

        var response = await _client.PostAsync("/projects", formData);

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/projects", response.Headers.Location?.ToString());

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();

        var createdProject = await dbContext.Set<Project>()
            .SingleOrDefaultAsync(p => p.Code == "TEST-001");

        Assert.NotNull(createdProject);
        Assert.Equal("Test Project", createdProject.Name);
        Assert.Equal("TEST-001", createdProject.Code);
        Assert.Equal(ProjectType.Category, createdProject.Type);
    }

    [Fact]
    public async Task Add_WithDuplicateCode_ReturnsBadRequestWithValidationError()
    {
        // First, create an existing project with a code
        using (var setupScope = _factory.Services.CreateScope())
        {
            var setupDbContext = setupScope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();
            var existingProject = new Project
            {
                Name = "Existing Project",
                Code = "DUPLICATE-001",
                Type = ProjectType.Category,
                CreatedAt = DateTime.UtcNow,
                HistoricalValues = []
            };
            setupDbContext.Add(existingProject);
            await setupDbContext.SaveChangesAsync();
        }

        var formData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("Name", "New Project"),
            new KeyValuePair<string, string>("Code", "DUPLICATE-001"),
            new KeyValuePair<string, string>("Type", "0"),
            new KeyValuePair<string, string>("CreatedDate", "2023-01-01"),
            new KeyValuePair<string, string>("CreatedTime", "12:00")
        ]);

        var response = await _client.PostAsync("/projects", formData);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Code &#x27;DUPLICATE-001&#x27; already exists. Please use a unique code.", content);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();

        var projectCount = await dbContext.Set<Project>()
            .CountAsync(p => p.Code == "DUPLICATE-001");
        Assert.Equal(1, projectCount);
    }

    [Fact]
    public async Task Add_WithCodeUsedByArchivedProject_ReturnsBadRequestWithValidationError()
    {
        // Create an archived project with a code
        using (var setupScope = _factory.Services.CreateScope())
        {
            var setupDbContext = setupScope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();
            var archivedProject = new Project
            {
                Name = "Archived Project",
                Code = "ARCHIVED-001",
                Type = ProjectType.Category,
                CreatedAt = DateTime.UtcNow,
                IsArchived = true,
                HistoricalValues = []
            };
            setupDbContext.Add(archivedProject);
            await setupDbContext.SaveChangesAsync();
        }

        var formData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("Name", "New Project"),
            new KeyValuePair<string, string>("Code", "ARCHIVED-001"),
            new KeyValuePair<string, string>("Type", "0"),
            new KeyValuePair<string, string>("CreatedDate", "2023-01-01"),
            new KeyValuePair<string, string>("CreatedTime", "12:00")
        ]);

        var response = await _client.PostAsync("/projects", formData);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Code &#x27;ARCHIVED-001&#x27; already exists. Please use a unique code.", content);
    }

    [Fact]
    public async Task EditForm_DisplaysCodeInForm()
    {
        int projectId;
        using (var setupScope = _factory.Services.CreateScope())
        {
            var setupDbContext = setupScope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();
            var project = new Project
            {
                Name = "Project With Code",
                Code = "EDIT-TEST-001",
                Type = ProjectType.Category,
                CreatedAt = DateTime.UtcNow,
                HistoricalValues = []
            };
            setupDbContext.Add(project);
            await setupDbContext.SaveChangesAsync();
            projectId = project.Id;
        }

        var response = await _client.GetAsync($"/projects/{projectId}");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("EDIT-TEST-001", content);
    }

    [Fact]
    public async Task Edit_WithNullCode_AllowsCodeAssignment()
    {
        int projectId;
        using (var setupScope = _factory.Services.CreateScope())
        {
            var setupDbContext = setupScope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();
            var project = new Project
            {
                Name = "Project Without Code",
                Code = null,
                Type = ProjectType.Category,
                CreatedAt = DateTime.UtcNow,
                HistoricalValues = []
            };
            setupDbContext.Add(project);
            await setupDbContext.SaveChangesAsync();
            projectId = project.Id;
        }

        var formData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("Name", "Project Without Code"),
            new KeyValuePair<string, string>("Code", "BACKFILL-001"),
            new KeyValuePair<string, string>("Type", "0"),
            new KeyValuePair<string, string>("CreatedDate", "2023-01-01"),
            new KeyValuePair<string, string>("CreatedTime", "12:00")
        ]);

        var response = await _client.PostAsync($"/projects/{projectId}", formData);

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();

        var updatedProject = await dbContext.Set<Project>()
            .SingleOrDefaultAsync(p => p.Id == projectId);

        Assert.NotNull(updatedProject);
        Assert.Equal("BACKFILL-001", updatedProject.Code);
    }

    [Fact]
    public async Task Edit_WithExistingCode_PreservesCode()
    {
        int projectId;
        using (var setupScope = _factory.Services.CreateScope())
        {
            var setupDbContext = setupScope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();
            var project = new Project
            {
                Name = "Project With Code",
                Code = "PRESERVE-001",
                Type = ProjectType.Category,
                CreatedAt = DateTime.UtcNow,
                HistoricalValues = []
            };
            setupDbContext.Add(project);
            await setupDbContext.SaveChangesAsync();
            projectId = project.Id;
        }

        var formData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("Name", "Updated Project Name"),
            new KeyValuePair<string, string>("Code", "ATTEMPTED-CHANGE"),
            new KeyValuePair<string, string>("Type", "0"),
            new KeyValuePair<string, string>("CreatedDate", "2023-01-01"),
            new KeyValuePair<string, string>("CreatedTime", "12:00")
        ]);

        var response = await _client.PostAsync($"/projects/{projectId}", formData);

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();

        var updatedProject = await dbContext.Set<Project>()
            .SingleOrDefaultAsync(p => p.Id == projectId);

        Assert.NotNull(updatedProject);
        Assert.Equal("PRESERVE-001", updatedProject.Code); // Code should NOT change
        Assert.Equal("Updated Project Name", updatedProject.Name); // Name should update
    }

    [Fact]
    public async Task Edit_WithNullCodeAndDuplicateCode_ReturnsBadRequestWithValidationError()
    {
        int project1Id;
        int project2Id;

        using (var setupScope = _factory.Services.CreateScope())
        {
            var setupDbContext = setupScope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();

            var project1 = new Project
            {
                Name = "Project 1",
                Code = "TAKEN-001",
                Type = ProjectType.Category,
                CreatedAt = DateTime.UtcNow,
                HistoricalValues = []
            };
            setupDbContext.Add(project1);

            var project2 = new Project
            {
                Name = "Project 2",
                Code = null,
                Type = ProjectType.Category,
                CreatedAt = DateTime.UtcNow,
                HistoricalValues = []
            };
            setupDbContext.Add(project2);

            await setupDbContext.SaveChangesAsync();
            project1Id = project1.Id;
            project2Id = project2.Id;
        }

        var formData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("Name", "Project 2"),
            new KeyValuePair<string, string>("Code", "TAKEN-001"),
            new KeyValuePair<string, string>("Type", "0"),
            new KeyValuePair<string, string>("CreatedDate", "2023-01-01"),
            new KeyValuePair<string, string>("CreatedTime", "12:00")
        ]);

        var response = await _client.PostAsync($"/projects/{project2Id}", formData);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Code &#x27;TAKEN-001&#x27; already exists. Please use a unique code.", content);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WookiepediaDbContext>();

        var project2Updated = await dbContext.Set<Project>()
            .SingleOrDefaultAsync(p => p.Id == project2Id);

        Assert.NotNull(project2Updated);
        Assert.Null(project2Updated.Code); // Code should still be null
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
