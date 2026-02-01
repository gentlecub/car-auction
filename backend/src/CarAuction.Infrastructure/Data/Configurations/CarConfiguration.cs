using CarAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarAuction.Infrastructure.Data.Configurations;

public class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.ToTable("Cars");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Brand)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.VIN)
            .HasMaxLength(17);

        builder.HasIndex(c => c.VIN)
            .IsUnique()
            .HasFilter("[VIN] IS NOT NULL");

        builder.Property(c => c.Color)
            .HasMaxLength(50);

        builder.Property(c => c.EngineType)
            .HasMaxLength(100);

        builder.Property(c => c.Transmission)
            .HasMaxLength(50);

        builder.Property(c => c.FuelType)
            .HasMaxLength(50);

        builder.Property(c => c.Description)
            .HasMaxLength(2000);

        builder.Property(c => c.Condition)
            .HasMaxLength(50);

        builder.Property(c => c.Features)
            .HasColumnType("TEXT");

        builder.HasIndex(c => new { c.Brand, c.Model, c.Year });
        builder.HasIndex(c => c.IsActive);
    }
}
