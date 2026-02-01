using CarAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarAuction.Infrastructure.Data.Configurations;

public class AuctionHistoryConfiguration : IEntityTypeConfiguration<AuctionHistory>
{
    public void Configure(EntityTypeBuilder<AuctionHistory> builder)
    {
        builder.ToTable("AuctionHistories");

        builder.HasKey(ah => ah.Id);

        builder.Property(ah => ah.FinalPrice)
            .HasPrecision(18, 2);

        builder.Property(ah => ah.Notes)
            .HasMaxLength(1000);

        builder.HasOne(ah => ah.Auction)
            .WithOne(a => a.History)
            .HasForeignKey<AuctionHistory>(ah => ah.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ah => ah.Winner)
            .WithMany()
            .HasForeignKey(ah => ah.WinnerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(ah => ah.CompletedAt);
    }
}
