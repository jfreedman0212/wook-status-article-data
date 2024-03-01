using System.ComponentModel.DataAnnotations;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using SlashPineTech.Forestry.Lifecycle;
using SlashPineTech.Forestry.ServiceModules;
using WookiepediaStatusArticleData.Database;

namespace WookiepediaStatusArticleData;

public class Startup(IConfiguration configuration, IWebHostEnvironment environment)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddProblemDetails();

        services.AddLifecycleActions();

        services.AddModules(typeof(Startup).Assembly, environment, configuration)
            .AddModule<DatabaseModule>("Database");

        services.AddAuth0WebAppAuthentication(options =>
        {
            options.Domain = configuration["Auth0:Domain"] ?? throw new InvalidOperationException();
            options.ClientId = configuration["Auth0:ClientId"] ?? throw new InvalidOperationException();
        });
        
        services.AddControllersWithViews();
        
        services.AddScoped<IStartupAction, SchemaMigrationAction>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });
        
        app.UseStaticFiles();
        
        app.UseHttpMethodOverride(new HttpMethodOverrideOptions
        {
            FormFieldName = "_method"
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