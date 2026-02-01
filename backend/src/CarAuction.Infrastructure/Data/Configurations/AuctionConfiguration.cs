using CarAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarAuction.Infrastructure.Data.Configurations;

public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
{
    public void Configure(EntityTypeBuilder<Auction> builder)
    {
        builder.ToTable("Auctions");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.StartingPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(a => a.ReservePrice)
            .HasPrecision(18, 2);

        builder.Property(a => a.MinimumBidIncrement)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(a => a.CurrentBid)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasOne(a => a.Car)
            .WithOne(c => c.Auction)
            .HasForeignKey<Auction>(a => a.CarId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.CurrentBidder)
            .WithMany()
            .HasForeignKey(a => a.CurrentBidderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.EndTime);
        builder.HasIndex(a => new { a.Status, a.EndTime });
    }
}
