using System.Security.Claims;
using Exit.exe.Repository.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Exit.exe.Web.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    private const string DefaultFrontendUrl = "https://localhost:5173";
    private const string DefaultScalarUrl = "/scalar/v1";

    [HttpGet("login/google")]
    [AllowAnonymous]
    public IActionResult LoginGoogle([FromQuery] string? returnUrl = null)
    {
        var safeReturnUrl = GetSafeReturnUrl(returnUrl);
        var callback = Url.Action(nameof(GoogleCallback), "Auth", new { returnUrl = safeReturnUrl });

        var props = signInManager.ConfigureExternalAuthenticationProperties("Google", callback!);
        return Challenge(props, "Google");
    }

    [HttpGet("login/google/callback")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GoogleCallback([FromQuery] string? returnUrl = null)
    {
        var safeReturnUrl = GetSafeReturnUrl(returnUrl);

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
            return Redirect(safeReturnUrl);

        var result = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);

        if (!result.Succeeded)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = string.IsNullOrWhiteSpace(email)
                ? null
                : await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = email ?? $"{info.LoginProvider}_{info.ProviderKey}",
                    Email = email,
                    EmailConfirmed = !string.IsNullOrWhiteSpace(email)
                };

                var created = await userManager.CreateAsync(user);
                if (!created.Succeeded)
                    return Redirect(safeReturnUrl);
            }

            var addLoginResult = await userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
                return Redirect(safeReturnUrl);

            await signInManager.SignInAsync(user, isPersistent: false);
        }

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        return Redirect(safeReturnUrl);
    }

    [HttpGet("me")]
    [AllowAnonymous]
    public async Task<ActionResult<MeResponse>> Me()
    {
        if (User?.Identity?.IsAuthenticated != true)
            return Ok(new MeResponse(false, null, null, null));

        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return Ok(new MeResponse(false, null, null, null));

        return Ok(new MeResponse(true, user.Id, user.Email, user.UserName));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }

    private static string GetSafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return DefaultFrontendUrl;

        if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var absoluteUri))
        {
            var allowedOrigins = new[]
            {
                "https://localhost:5173",
                "http://localhost:5173",
                "https://localhost:7007",
                "http://localhost:7007"
            };

            var origin = absoluteUri.GetLeftPart(UriPartial.Authority);

            if (allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                return returnUrl;
        }

        if (returnUrl.StartsWith("/", StringComparison.Ordinal))
            return DefaultScalarUrl;

        return DefaultFrontendUrl;
    }

    public sealed record MeResponse(bool IsAuthenticated, string? UserId, string? Email, string? UserName);
}