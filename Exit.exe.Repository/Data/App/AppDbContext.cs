using Exit.exe.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Exit.exe.Repository.Data.App;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Puzzle> Puzzles => Set<Puzzle>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        AppDbContextInitializer.Seed(modelBuilder);
    }
}