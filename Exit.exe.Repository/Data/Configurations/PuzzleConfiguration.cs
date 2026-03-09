using Exit.exe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exit.exe.Repository.Data.Configurations;

public sealed class PuzzleConfiguration : IEntityTypeConfiguration<Puzzle>
{
    public void Configure(EntityTypeBuilder<Puzzle> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.GameType)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(p => p.Payload)
               .IsRequired();

        builder.HasIndex(p => p.GameType);
    }
}
