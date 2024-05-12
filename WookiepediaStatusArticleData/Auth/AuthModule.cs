using System.Security.Claims;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SlashPineTech.Forestry.ServiceModules;

namespace WookiepediaStatusArticleData.Auth;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class AuthModule : IServiceModule
{
    public required string Authority { get; init; }
    public required string Audience { get; init; }
    public required string Issuer { get; init; }
    
    public void Configure(IServiceCollection services, IServiceConfigurationContext ctx)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = Authority;
                options.Audience = Audience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });

        services.AddAuthorization();
    }
}