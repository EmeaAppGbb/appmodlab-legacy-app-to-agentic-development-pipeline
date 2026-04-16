using KeystoneInsurance.Modern.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KeystoneInsurance.Modern.Data.Configurations;

public class RateFactorConfiguration : IEntityTypeConfiguration<RateFactor>
{
    public void Configure(EntityTypeBuilder<RateFactor> builder)
    {
        builder.ToTable("RateFactors");
        builder.HasKey(r => r.FactorId);

        builder.Property(r => r.FactorType).HasMaxLength(50).IsRequired();
        builder.Property(r => r.FactorCode).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(200);
        builder.Property(r => r.FactorValue).HasPrecision(10, 4);
        builder.Property(r => r.StateCode).HasMaxLength(2);
        builder.Property(r => r.TerritoryCode).HasMaxLength(10);

        builder.Property(r => r.CreatedDate).HasDefaultValueSql("GETDATE()");

        builder.HasIndex(r => new { r.FactorType, r.StateCode });
        builder.HasIndex(r => new { r.EffectiveDate, r.ExpirationDate });
    }
}
