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
    // GET /api/auth/login/google?returnUrl=...
    [HttpGet("login/google")]
    [AllowAnonymous]
    public IActionResult LoginGoogle([FromQuery] string returnUrl = "/scalar/v1")
    {
        var callback = Url.Action(nameof(GoogleCallback), "Auth", new { returnUrl });
        var props = signInManager.ConfigureExternalAuthenticationProperties("Google", callback);
        return Challenge(props, "Google");
    }

    // GET /api/auth/login/google/callback
    [HttpGet("login/google/callback")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GoogleCallback([FromQuery] string returnUrl = "/scalar/v1")
    {
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
            return Redirect(returnUrl);

        var result = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        if (!result.Succeeded)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = string.IsNullOrWhiteSpace(email) ? null : await userManager.FindByEmailAsync(email);

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
                    return Redirect(returnUrl);
            }

            await userManager.AddLoginAsync(user, info);
            await signInManager.SignInAsync(user, isPersistent: false);
        }

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        return Redirect(returnUrl);
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

    public sealed record MeResponse(bool IsAuthenticated, string? UserId, string? Email, string? UserName);
}