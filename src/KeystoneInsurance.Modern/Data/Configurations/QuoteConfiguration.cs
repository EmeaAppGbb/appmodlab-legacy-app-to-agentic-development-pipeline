using KeystoneInsurance.Modern.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KeystoneInsurance.Modern.Data.Configurations;

public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("Quotes");
        builder.HasKey(q => q.QuoteId);

        builder.Property(q => q.QuoteNumber).HasMaxLength(30).IsRequired();
        builder.HasIndex(q => q.QuoteNumber).IsUnique();

        builder.Property(q => q.Status).HasMaxLength(20).IsRequired();
        builder.Property(q => q.PropertyAddress).HasMaxLength(200).IsRequired();
        builder.Property(q => q.City).HasMaxLength(100).IsRequired();
        builder.Property(q => q.StateCode).HasMaxLength(2).IsRequired();
        builder.Property(q => q.ZipCode).HasMaxLength(10).IsRequired();
        builder.Property(q => q.CountyName).HasMaxLength(100);
        builder.Property(q => q.PropertyValue).HasPrecision(15, 2);
        builder.Property(q => q.ConstructionType).HasMaxLength(50).IsRequired();
        builder.Property(q => q.OccupancyType).HasMaxLength(100).IsRequired();
        builder.Property(q => q.RoofType).HasMaxLength(50);

        builder.Property(q => q.CoverageLimit).HasPrecision(15, 2);
        builder.Property(q => q.Deductible).HasPrecision(15, 2);
        builder.Property(q => q.BusinessInterruptionLimit).HasPrecision(15, 2);
        builder.Property(q => q.PriorClaimsTotalAmount).HasPrecision(15, 2);

        builder.Property(q => q.BasePremium).HasPrecision(10, 2);
        builder.Property(q => q.TotalPremium).HasPrecision(10, 2);

        builder.Property(q => q.CreatedDate).HasDefaultValueSql("GETDATE()");

        builder.HasIndex(q => q.ClientId);
        builder.HasIndex(q => q.Status);
        builder.HasIndex(q => q.StateCode);

        builder.HasOne(q => q.UnderwritingDecision)
            .WithOne(u => u.Quote)
            .HasForeignKey<UnderwritingDecision>(u => u.QuoteId);
    }
}
