using Exit.exe.Repository.Auth;
using Exit.exe.Repository.Data.App;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

services.AddControllers();

var conn = config.GetConnectionString("DefaultConnection")!;
var migrationsAssembly = typeof(AuthDbContext).Assembly.FullName;

services.AddDbContext<AuthDbContext>(opt =>
    opt.UseSqlServer(conn, o => o.MigrationsAssembly(migrationsAssembly)));

services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(conn, o => o.MigrationsAssembly(migrationsAssembly)));

services
    .AddIdentityCore<ApplicationUser>(opt => opt.User.RequireUniqueEmail = true)
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddCookie(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ExternalScheme)
    .AddGoogle("Google", o =>
    {
        o.ClientId = config["Authentication:Google:ClientId"]!;
        o.ClientSecret = config["Authentication:Google:ClientSecret"]!;
    })
    .AddMicrosoftAccount("Microsoft", o =>
    {
        o.ClientId = config["Authentication:Microsoft:ClientId"]!;
        o.ClientSecret = config["Authentication:Microsoft:ClientSecret"]!;
    });

services.AddAuthorization();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
await app.RunAsync();