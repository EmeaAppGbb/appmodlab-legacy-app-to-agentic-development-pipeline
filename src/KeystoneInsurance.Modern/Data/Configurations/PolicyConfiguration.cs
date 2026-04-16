using KeystoneInsurance.Modern.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KeystoneInsurance.Modern.Data.Configurations;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");
        builder.HasKey(p => p.PolicyId);

        builder.Property(p => p.PolicyNumber).HasMaxLength(30).IsRequired();
        builder.HasIndex(p => p.PolicyNumber).IsUnique();

        builder.Property(p => p.Status).HasMaxLength(30).IsRequired();
        builder.Property(p => p.AnnualPremium).HasPrecision(10, 2);
        builder.Property(p => p.PaymentPlan).HasMaxLength(20).IsRequired();
        builder.Property(p => p.InstallmentAmount).HasPrecision(10, 2);
        builder.Property(p => p.CoverageLimit).HasPrecision(15, 2);
        builder.Property(p => p.Deductible).HasPrecision(15, 2);
        builder.Property(p => p.CoverageType).HasMaxLength(50).HasDefaultValue("Commercial Property");

        builder.Property(p => p.BusinessInterruptionLimit).HasPrecision(15, 2);
        builder.Property(p => p.FloodLimit).HasPrecision(15, 2);
        builder.Property(p => p.EarthquakeLimit).HasPrecision(15, 2);
        builder.Property(p => p.CededPremium).HasPrecision(10, 2);
        builder.Property(p => p.ReinsuranceTreatyId).HasMaxLength(50);
        builder.Property(p => p.CancellationReason).HasMaxLength(500);
        builder.Property(p => p.ReturnPremium).HasPrecision(10, 2);
        builder.Property(p => p.PolicyDocumentPath).HasMaxLength(500);

        builder.Property(p => p.IssueDate).HasDefaultValueSql("GETDATE()");
        builder.Property(p => p.CreatedDate).HasDefaultValueSql("GETDATE()");

        builder.HasIndex(p => p.QuoteId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.EffectiveDate);

        builder.HasOne(p => p.Quote)
            .WithOne(q => q.Policy)
            .HasForeignKey<Policy>(p => p.QuoteId);

        builder.HasMany(p => p.Endorsements)
            .WithOne(e => e.Policy)
            .HasForeignKey(e => e.PolicyId);
    }
}
