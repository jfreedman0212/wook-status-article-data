using Auth0.AspNetCore.Authentication;
using JetBrains.Annotations;
using SlashPineTech.Forestry.ServiceModules;

namespace WookiepediaStatusArticleData.Auth;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class AuthModule : IServiceModule
{
    public required string Domain { get; init; }
    public required string ClientId { get; init; }

    public void Configure(IServiceCollection services, IServiceConfigurationContext ctx)
    {
        services.AddAuth0WebAppAuthentication(options =>
        {
            options.Domain = Domain;
            options.ClientId = ClientId;
        });

        services.AddAuthorization();
    }
}