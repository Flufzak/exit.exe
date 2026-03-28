using Exit.exe.Application.Behaviors;
using Exit.exe.Application.Contracts;
using Exit.exe.Application.Features.Games.Queries;
using Exit.exe.Repository.Auth;
using Exit.exe.Repository.Data.App;
using Exit.exe.Repository.Repositories;
using Exit.exe.Web.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

// Explicitly load user secrets in Development
if (builder.Environment.IsDevelopment())
{
    config.AddUserSecrets(Assembly.GetExecutingAssembly());
}

// Controllers register
services.AddControllers();
services.AddExceptionHandler<GlobalExceptionHandler>();
services.AddProblemDetails();

// OpenAPI document generation
services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "Exit.exe API";
        document.Info.Version = "v1";
        document.Info.Description =
            "Escape-room puzzle game API.\n\n" +
            "\ud83d\udd10 **Cookie-based auth** \u2014 " +
            "[Login with Google](/api/auth/login/google) " +
            "to authenticate, then use *Try it* on protected endpoints.";

        return Task.CompletedTask;
    });
});

// CQRS/MediatR register
var applicationAssembly = typeof(GetGamesQuery).Assembly;

services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(applicationAssembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

services.AddValidatorsFromAssembly(applicationAssembly);

// ---- Repositories ----
services.AddScoped<IPuzzleRepository, PuzzleRepository>();
services.AddScoped<ISessionRepository, SessionRepository>();

// ---- AI Core (primary) ----
services.Configure<AiCoreOptions>(config.GetSection("AI:Core"));

services.AddHttpClient<IAiService, AiCoreService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

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
        sql.EnableRetryOnFailure();
    }));

services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(conn, sql =>
    {
        sql.MigrationsAssembly(migrationsAssembly);
        sql.MigrationsHistoryTable("__AppMigrationsHistory");
        sql.EnableRetryOnFailure();
    }));

// ---- Identity (cookie-based) ----
services
    .AddIdentityCore<ApplicationUser>(opt => { opt.User.RequireUniqueEmail = true; })
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// ---- CORS for SPA ----
if (builder.Environment.IsDevelopment())
{
    services.AddCors(o => o.AddPolicy("Spa", p =>
        p.SetIsOriginAllowed(_ => true)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()));
}
else
{
    var spaOrigins = config.GetSection("Auth:SpaOrigins")
        .GetChildren()
        .Select(c => c.Value)
        .Where(v => !string.IsNullOrWhiteSpace(v))
        .ToArray();

    services.AddCors(o => o.AddPolicy("Spa", p =>
        p.WithOrigins(spaOrigins!)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()));
}

// ---- Auth schemes + external providers ----
var googleClientId = config["Authentication:Google:ClientId"];
var googleClientSecret = config["Authentication:Google:ClientSecret"];

var authBuilder = services
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
    });

// Google OAuth (chain on same builder — no second AddAuthentication() call)
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authBuilder.AddGoogle("Google", o =>
    {
        o.ClientId = googleClientId;
        o.ClientSecret = googleClientSecret;
        o.SignInScheme = IdentityConstants.ExternalScheme;
        o.SaveTokens = true;
    });
}

services.AddAuthorization();

var app = builder.Build();

// Middleware pipeline
app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseCors("Spa");         // must be before auth for browser calls
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("Exit.exe API");
    options.WithTheme(ScalarTheme.DeepSpace);
    options.WithDefaultHttpClient(ScalarTarget.Shell, ScalarClient.Curl);
    options.ForceDarkMode();
});

app.MapControllers();

await app.RunAsync();