using Exit.exe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exit.exe.Repository.Data.Configurations;

public sealed class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
{
    public void Configure(EntityTypeBuilder<GameSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId)
               .HasMaxLength(450)
               .IsRequired();

        builder.Property(s => s.GuessedLetters)
               .HasMaxLength(200);

        builder.Property(s => s.Status)
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.HasIndex(s => s.UserId);

        builder.HasOne(s => s.Puzzle)
               .WithMany(p => p.Sessions)
               .HasForeignKey(s => s.PuzzleId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
