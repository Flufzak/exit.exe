using Microsoft.EntityFrameworkCore;

namespace Exit.exe.Repository.Data.App;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{

}