# Agent Guide for WookiepediaStatusArticleData

This document provides guidelines for AI coding agents working on the WookiepediaStatusArticleData project.

## Project Overview

This is an ASP.NET Core 8.0 web application using Entity Framework Core with PostgreSQL for managing Wookieepedia status article nominations, awards, and projects.

**Tech Stack:**
- .NET 8.0 (C#)
- ASP.NET Core MVC with Razor views
- Entity Framework Core with PostgreSQL (Npgsql)
- xUnit for testing
- Auth0 for authentication
- HTMX for frontend interactivity
- DbUp for database migrations

## Build, Test, and Run Commands

Run `dotnet` commands from the `WookiepediaStatusArticleData` folder.

### Building
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build WookiepediaStatusArticleData/WookiepediaStatusArticleData.csproj

# Build in Release mode
dotnet build -c Release
```

### Testing
```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test -v normal

# Run a single test class
dotnet test --filter "FullyQualifiedName~ProjectsControllerTest"

# Run a specific test method
dotnet test --filter "FullyQualifiedName~ProjectsControllerTest.Add_WithValidUniqueCode_CreatesProjectSuccessfully"

# Run tests matching a name pattern
dotnet test --filter "Name~Add"

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Running
```bash
# Run the application
dotnet run --project WookiepediaStatusArticleData/WookiepediaStatusArticleData.csproj

# Run with specific environment
dotnet run --project WookiepediaStatusArticleData/WookiepediaStatusArticleData.csproj --environment Development
```

### Database Migrations
```bash
# Create a new migration (use the helper script)
./create_migration.sh MigrationName

# Migrations are in Database/Migrate/*.sql and applied via DbUp on startup
```

## Code Style Guidelines

### File Organization
- Controllers: `WookiepediaStatusArticleData/Controllers/`
- Models (entities): `WookiepediaStatusArticleData/Nominations/{Domain}/`
- ViewModels/Forms: `WookiepediaStatusArticleData/Models/`
- Services/Actions: `WookiepediaStatusArticleData/Services/{Domain}/`
- Database: `WookiepediaStatusArticleData/Database/`
- Tests: `WookiepediaStatusArticleData.Tests/`

### Naming Conventions
- **Files**: PascalCase matching the class name (e.g., `ProjectsController.cs`)
- **Classes**: PascalCase (e.g., `ProjectsController`, `CreateProjectAction`)
- **Interfaces**: PascalCase with `I` prefix (e.g., `IProjectAwardCalculation`)
- **Methods**: PascalCase (e.g., `ExecuteAsync`, `Archive`)
- **Properties**: PascalCase (e.g., `Name`, `CreatedAt`)
- **Private fields**: camelCase with `_` prefix (e.g., `_fixture`, `_client`)
- **Parameters**: camelCase (e.g., `cancellationToken`, `form`)
- **Local variables**: camelCase (e.g., `project`, `affectedGroups`)

### Namespace and Using Directives
- One namespace per file using file-scoped namespace syntax
- Group usings: System namespaces, then third-party, then local
- Remove unused usings
- Use explicit namespace declarations (no global usings except those in csproj)

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Services;

namespace WookiepediaStatusArticleData.Controllers;

[Authorize]
public class ExampleController : Controller
{
    // ...
}
```

### Types and Nullability
- **Nullable reference types enabled**: `<Nullable>enable</Nullable>` is set
- Use `?` for nullable reference types (e.g., `string?`, `IList<T>?`)
- Use `required` modifier for required properties (e.g., `public required string Name { get; set; }`)
- Avoid null-forgiving operator (`!`) unless you're certain the value is non-null

### Primary Constructors
- Use primary constructors for dependency injection in controllers and services
```csharp
public class ProjectsController(WookiepediaDbContext db) : Controller
{
    // db is automatically available as a field
}
```

### Method Signatures
- Always include `CancellationToken` parameter for async database operations
- Pass `CancellationToken` to all EF Core methods
- Use `[FromServices]` attribute for action-scoped service injection
- Use `[FromRoute]`, `[FromQuery]`, `[FromForm]` attributes to be explicit

```csharp
[HttpPost("{id:int}")]
public async Task<IActionResult> Edit(
    [FromRoute] int id,
    [FromForm] ProjectForm form,
    [FromServices] EditProjectAction action,
    CancellationToken cancellationToken
)
{
    // ...
}
```

### Error Handling and Validation
- Use `ValidationException` for domain validation errors
- Add errors to `ModelState` in controllers
- Return appropriate HTTP status codes (400 for validation, 404 for not found)
- Set `Response.StatusCode` before returning views with errors

```csharp
try
{
    await action.ExecuteAsync(form, cancellationToken);
    await db.SaveChangesAsync(cancellationToken);
    return RedirectToAction("Index");
}
catch (ValidationException validationException)
{
    foreach (var issue in validationException.Issues)
    {
        ModelState.AddModelError(issue.Name, issue.Message);
    }
    Response.StatusCode = 400;
    return View("FormName", form);
}
```

### Database Operations
- Use Entity Framework Core for all database access
- Always pass `CancellationToken` to async EF methods
- Use transactions for multi-step operations: `await db.Database.BeginTransactionAsync(cancellationToken)`
- Use `Include()` for eager loading related entities
- Use raw SQL sparingly, with interpolated strings for parameterization: `$"..."` in ExecuteSqlAsync

```csharp
var project = await db.Set<Project>()
    .Include(it => it.HistoricalValues)
    .SingleOrDefaultAsync(it => it.Id == id, cancellationToken);
```

### Testing Guidelines
- Test class naming: `{ClassUnderTest}Test` (e.g., `ProjectsControllerTest`)
- Use xUnit's `[Fact]` attribute for tests
- Use `IClassFixture<T>` for shared test context
- Use Testcontainers for integration tests with PostgreSQL
- Test method naming: descriptive, action-oriented (e.g., `Add_WithValidUniqueCode_CreatesProjectSuccessfully`)
- Use `WebApplicationFactory<Program>` for controller integration tests
- Always clean up resources with `IAsyncLifetime`

```csharp
public class ExampleTest : IClassFixture<PostgresTestFixture>, IAsyncLifetime
{
    [Fact]
    public async Task MethodName_Scenario_ExpectedOutcome()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

### Formatting
- Use 4 spaces for indentation (not tabs)
- Opening braces on new line for types and methods
- Use expression-bodied members for simple properties: `public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);`
- Use collection expressions: `[item1, item2]` instead of `new[] { item1, item2 }` where appropriate
- Prefer `var` for local variables when type is obvious

### Comments
- Use XML doc comments (`///`) for public APIs
- Keep inline comments minimal and explain "why", not "what"
- Add comments for complex business logic or non-obvious decisions

## Architecture Patterns

### Controllers
- Keep controllers thin; delegate business logic to action classes
- Use dependency injection for services and actions
- Return appropriate action results (`View`, `RedirectToAction`, `NotFound`, `Ok`)

### Actions (Service Layer)
- Create action classes for business operations (e.g., `CreateProjectAction`, `EditProjectAction`)
- Register actions as scoped services
- Actions should be focused on a single responsibility

### Domain Models
- Place entity classes in `Nominations/{Domain}/` folders
- Use `required` properties for non-nullable fields
- Include domain methods on entities (e.g., `Archive()`)

### Validation
- Use `ValidationException` with `ValidationIssue` for domain validation
- Perform validation in action classes before persistence
- Return specific, user-friendly error messages

## Common Pitfalls to Avoid
1. **Don't forget CancellationToken**: Always pass it to async EF Core methods
2. **Don't use blocking calls**: Use `async`/`await` consistently
3. **Don't forget transactions**: Use transactions for multi-step database operations
4. **Don't ignore ModelState**: Always check `ModelState.IsValid` in controller actions
5. **Don't hardcode status codes**: Use appropriate HTTP status code constants or set `Response.StatusCode`
6. **Don't use deprecated patterns**: Use file-scoped namespaces, primary constructors, and collection expressions

## Additional Notes
- The application uses Auth0 for authentication; controllers are decorated with `[Authorize]`
- HTMX is used for dynamic UI updates
- Database migrations are SQL files in `Database/Migrate/` and applied via DbUp
- Feature flags are managed with `Microsoft.FeatureManagement`
