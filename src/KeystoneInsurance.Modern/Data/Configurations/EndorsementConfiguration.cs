using KeystoneInsurance.Modern.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KeystoneInsurance.Modern.Data.Configurations;

public class EndorsementConfiguration : IEntityTypeConfiguration<Endorsement>
{
    public void Configure(EntityTypeBuilder<Endorsement> builder)
    {
        builder.ToTable("Endorsements");
        builder.HasKey(e => e.EndorsementId);

        builder.Property(e => e.EndorsementNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.EndorsementNumber).IsUnique();

        builder.Property(e => e.EndorsementType).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
        builder.Property(e => e.PremiumChange).HasPrecision(10, 2);
        builder.Property(e => e.NewCoverageLimit).HasPrecision(15, 2);
        builder.Property(e => e.NewDeductible).HasPrecision(15, 2);
        builder.Property(e => e.EndorsementDocumentPath).HasMaxLength(500);

        builder.Property(e => e.RequestDate).HasDefaultValueSql("GETDATE()");
        builder.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");

        builder.HasIndex(e => e.PolicyId);
    }
}
