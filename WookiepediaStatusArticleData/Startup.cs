using System.Text.Json.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.FeatureManagement;
using SlashPineTech.Forestry.Lifecycle;
using SlashPineTech.Forestry.ServiceModules;
using WookiepediaStatusArticleData.Auth;
using WookiepediaStatusArticleData.Database;
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

        services.AddModules(typeof(Startup).Assembly, environment, configuration)
            .AddModule<DatabaseModule>("Database")
            .AddModule<AuthModule>("Auth");

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new DateOnlyConverter());
                options.JsonSerializerOptions.Converters.Add(new TimeOnlyConverter());
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        services.AddScoped<IStartupAction, SchemaMigrationAction>();

        services.AddScoped<NominatorValidator>();
        services.AddScoped<EditNominatorAction>();
        
        services.AddScoped<CreateProjectAction>();
        services.AddScoped<EditProjectAction>();
        services.AddScoped<ProjectValidator>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseCors();
        
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });
        
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute("default", "{controller=Projects}/{action=Index}/{id?}");
        });
    }
}