using Exit.exe.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Exit.exe.Repository.Data;

public static class AppDbContextInitializer
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Puzzle>().HasData(
            new Puzzle
            {
                Id = new Guid("a1b2c3d4-0001-0000-0000-000000000001"),
                GameType = "hangman",
                Payload = """{"word":"KAZIMIR","description":"The name of the ancient sect that imprisoned you","category":"Names","maxAttempts":6}""",
                CreatedAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Puzzle
            {
                Id = new Guid("a1b2c3d4-0001-0000-0000-000000000002"),
                GameType = "hangman",
                Payload = """{"word":"SACRIFICE","description":"The ritual demands this from the chosen one","category":"Concepts","maxAttempts":6}""",
                CreatedAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Puzzle
            {
                Id = new Guid("a1b2c3d4-0001-0000-0000-000000000003"),
                GameType = "hangman",
                Payload = """{"word":"ASCENSION","description":"What the priests believe awaits beyond death","category":"Concepts","maxAttempts":6}""",
                CreatedAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Puzzle
            {
                Id = new Guid("a1b2c3d4-0001-0000-0000-000000000004"),
                GameType = "hangman",
                Payload = """{"word":"OFFERING","description":"You are this to the ancient order","category":"Concepts","maxAttempts":6}""",
                CreatedAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
