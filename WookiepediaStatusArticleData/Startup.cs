using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.FeatureManagement;
using SlashPineTech.Forestry.Lifecycle;
using SlashPineTech.Forestry.ServiceModules;
using WookiepediaStatusArticleData.Auth;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Services.Awards;
using WookiepediaStatusArticleData.Services.Nominations;
using WookiepediaStatusArticleData.Services.Nominators;
using WookiepediaStatusArticleData.Services.Projects;

namespace WookiepediaStatusArticleData;

public class Startup(IConfiguration configuration, IWebHostEnvironment environment)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddProblemDetails();

        services.AddFeatureManagement(configuration.GetSection("Features"));
        services.AddLifecycleActions();

        services
            .AddModules(typeof(Startup).Assembly, environment, configuration)
            .AddModule<DatabaseModule>("Database")
            .AddModule<AuthModule>("Auth");

        services.AddControllersWithViews();

        services.AddScoped<IStartupAction, SchemaMigrationAction>();

        services.AddScoped<NominationLookup>();
        services.AddScoped<NominationImporter>();
        services.AddScoped<NominationCsvRowProcessor>();

        services.AddScoped<NominatorValidator>();
        services.AddScoped<EditNominatorAction>();

        services.AddScoped<CreateProjectAction>();
        services.AddScoped<EditProjectAction>();
        services.AddScoped<ProjectValidator>();
        services.AddScoped<TopAwardsLookup>();
        services.AddScoped<TopProjectAwardsLookup>();

        services.AddScoped<IAwardGenerator, StaticAwardGenerator>();
        services.AddScoped<IAwardGenerator, ProjectsAwardGenerator>();
        
        services.AddScoped<IProjectAwardCalculation, NominationsByProjectAwardGenerator>();
        services.AddScoped<IProjectAwardCalculation, PointsByProjectAwardGenerator>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/home/error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
            
            app.Use((context, next) =>
            {
                context.Request.Scheme = "https";
                return next();
            });
        }

        app.UseForwardedHeaders(
            new ForwardedHeadersOptions
            {
                ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            }
        );

        app.UseStaticFiles();

        app.UseHttpMethodOverride(new HttpMethodOverrideOptions { FormFieldName = "_method" });

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
        });
    }
}

