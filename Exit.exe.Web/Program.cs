using Exit.exe.Repository.Auth;
using Exit.exe.Repository.Data.App;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

services.AddControllers();

// ---- DB ----
var conn = config.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(conn))
    throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");

var migrationsAssembly = typeof(AuthDbContext).Assembly.FullName;

// Two DbContexts in one physical DB => different history tables (important for later)
services.AddDbContext<AuthDbContext>(opt =>
    opt.UseSqlServer(conn, sql =>
    {
        sql.MigrationsAssembly(migrationsAssembly);
        sql.MigrationsHistoryTable("__AuthMigrationsHistory");
    }));

services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(conn, sql =>
    {
        sql.MigrationsAssembly(migrationsAssembly);
        sql.MigrationsHistoryTable("__AppMigrationsHistory");
    }));

// ---- Identity (cookie-based) ----
services
    .AddIdentityCore<ApplicationUser>(opt => { opt.User.RequireUniqueEmail = true; })
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// ---- CORS for SPA ----
var spaOrigins = config.GetSection("Auth:SpaOrigins")
    .GetChildren()
    .Select(c => c.Value)
    .Where(v => !string.IsNullOrWhiteSpace(v))
    .ToArray();

services.AddCors(o =>
{
    o.AddPolicy("Spa", p =>
        p.WithOrigins(spaOrigins!)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

// ---- Auth schemes + external providers ----
services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        // Main login cookie
        options.Cookie.Name = "exitexe.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;

        // For APIs: return 401 instead of redirect HTML
        options.Events.OnRedirectToLogin = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        };
    })
    .AddCookie(IdentityConstants.ExternalScheme, options =>
    {
        // Short-lived cookie used during external login handshake
        options.Cookie.Name = "exitexe.external";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddGoogle("Google", o =>
    {
        o.ClientId = config["Authentication:Google:ClientId"]!;
        o.ClientSecret = config["Authentication:Google:ClientSecret"]!;
        o.SignInScheme = IdentityConstants.ExternalScheme;
        o.SaveTokens = true;
    });

services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("Spa");         // must be before auth for browser calls
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();