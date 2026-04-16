using KeystoneInsurance.Modern.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KeystoneInsurance.Modern.Data.Configurations;

public class UnderwritingDecisionConfiguration : IEntityTypeConfiguration<UnderwritingDecision>
{
    public void Configure(EntityTypeBuilder<UnderwritingDecision> builder)
    {
        builder.ToTable("UnderwritingDecisions");
        builder.HasKey(u => u.UWId);

        builder.Property(u => u.Decision).HasMaxLength(30).IsRequired();
        builder.Property(u => u.RiskScore).HasPrecision(5, 2);
        builder.Property(u => u.CatastrophePML).HasPrecision(15, 2);
        builder.Property(u => u.CatastropheZoneRating).HasMaxLength(20);
        builder.Property(u => u.ConstructionRating).HasMaxLength(20);
        builder.Property(u => u.OccupancyRating).HasMaxLength(20);
        builder.Property(u => u.ProtectionRating).HasMaxLength(20);
        builder.Property(u => u.LossHistoryRating).HasMaxLength(20);

        builder.Property(u => u.CreatedDate).HasDefaultValueSql("GETDATE()");

        builder.HasIndex(u => u.QuoteId);
    }
}
