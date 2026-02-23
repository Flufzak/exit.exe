using System.Security.Claims;
using Exit.exe.Repository.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Exit.exe.Web.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration config) : ControllerBase
{
    private static readonly HashSet<string> SupportedProviders =
        new(StringComparer.OrdinalIgnoreCase) { "Google" };
    private readonly IConfiguration _config = config;

    // GET /api/auth/external/{provider}?returnUrl=...
    [HttpGet("external/{provider}")]
    [AllowAnonymous]
    public IActionResult External([FromRoute] string provider, [FromQuery] string? returnUrl = null)
    {
        if (!SupportedProviders.Contains(provider))
            return BadRequest(new { error = "Unsupported provider. Use Google or Microsoft." });

        var redirectUrl = Url.Action(nameof(ExternalCallback), "Auth", new { provider, returnUrl });
        if (string.IsNullOrWhiteSpace(redirectUrl))
            return Problem("Could not generate callback URL.");

        var props = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(props, provider);
    }

    // GET /api/auth/external/{provider}/callback?returnUrl=...
    [HttpGet("external/{provider}/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalCallback(
        [FromRoute] string provider,
        [FromQuery] string? returnUrl = null,
        [FromQuery] string? remoteError = null)
    {
        var safeReturnUrl = GetSafeReturnUrl(returnUrl);

        if (!SupportedProviders.Contains(provider))
            return Redirect(safeReturnUrl);

        if (!string.IsNullOrWhiteSpace(remoteError))
            return Redirect(QueryHelpers.AddQueryString(safeReturnUrl, "authError", remoteError));

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
            return Redirect(QueryHelpers.AddQueryString(safeReturnUrl, "authError", "Missing external login info"));

        var signInResult = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);

        if (!signInResult.Succeeded)
        {
            var email = GetEmail(info.Principal);
            var user = await FindOrCreateUserAsync(email, info);

            var link = await userManager.AddLoginAsync(user, info);
            if (!link.Succeeded)
            {
                var err = link.Errors.FirstOrDefault()?.Description ?? "Could not link external login";
                return Redirect(QueryHelpers.AddQueryString(safeReturnUrl, "authError", err));
            }

            await signInManager.SignInAsync(user, isPersistent: false);
        }

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        return Redirect(safeReturnUrl);
    }

    // GET /api/auth/me
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

    // POST /api/auth/logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }

    private string GetSafeReturnUrl(string? returnUrl)
    {
        var fallback = _config["Auth:DefaultReturnUrl"] ?? "/";

        if (string.IsNullOrWhiteSpace(returnUrl))
            return fallback;

        if (Url.IsLocalUrl(returnUrl))
            return returnUrl;

        var allowed = _config.GetSection("Auth:AllowedReturnUrlPrefixes")
            .GetChildren()
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToArray();

        foreach (var prefix in allowed)
        {
            if (returnUrl.StartsWith(prefix!, StringComparison.OrdinalIgnoreCase))
                return returnUrl;
        }

        return fallback;
    }

    private static string? GetEmail(ClaimsPrincipal principal)
        => principal.FindFirstValue(ClaimTypes.Email)
           ?? principal.FindFirstValue("email")
           ?? principal.FindFirstValue("preferred_username");

    private async Task<ApplicationUser> FindOrCreateUserAsync(string? email, ExternalLoginInfo info)
    {
        ApplicationUser? user = null;

        if (!string.IsNullOrWhiteSpace(email))
            user = await userManager.FindByEmailAsync(email);

        if (user is not null)
            return user;

        var userName = !string.IsNullOrWhiteSpace(email)
            ? email
            : $"{info.LoginProvider}_{info.ProviderKey}";

        user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = !string.IsNullOrWhiteSpace(email)
        };

        var created = await userManager.CreateAsync(user);
        if (!created.Succeeded)
        {
            var err = created.Errors.FirstOrDefault()?.Description ?? "Could not create user";
            throw new InvalidOperationException(err);
        }

        return user;
    }

    public sealed record MeResponse(bool IsAuthenticated, string? UserId, string? Email, string? UserName);
}