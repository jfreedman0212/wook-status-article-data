using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WookiepediaStatusArticleData.Controllers;

[Route("Account")]
public class AccountController : Controller
{
    [HttpGet("Login")]
    public async Task Login([FromQuery] string? returnUrl)
    {
        var properties = new LoginAuthenticationPropertiesBuilder()
            .WithRedirectUri((returnUrl ?? Url.Action("Index", "Home")) ?? throw new InvalidOperationException())
            .Build();

        await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, properties);
    }

    [Authorize]
    [HttpGet("Logout")]
    public async Task Logout()
    {
        var properties = new LoginAuthenticationPropertiesBuilder()
            .WithRedirectUri(Url.Action("Index", "Home") ?? throw new InvalidOperationException())
            .Build();

        await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, properties);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}