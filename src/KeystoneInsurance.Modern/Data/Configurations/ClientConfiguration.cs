using KeystoneInsurance.Modern.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KeystoneInsurance.Modern.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");
        builder.HasKey(c => c.ClientId);

        builder.Property(c => c.ClientNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(c => c.ClientNumber).IsUnique();

        builder.Property(c => c.BusinessName).HasMaxLength(200).IsRequired();
        builder.Property(c => c.ContactFirstName).HasMaxLength(100);
        builder.Property(c => c.ContactLastName).HasMaxLength(100);
        builder.Property(c => c.Email).HasMaxLength(100);
        builder.Property(c => c.Phone).HasMaxLength(20);
        builder.Property(c => c.BusinessType).HasMaxLength(100);
        builder.Property(c => c.FederalTaxId).HasMaxLength(11);

        builder.Property(c => c.MailingAddress).HasMaxLength(200);
        builder.Property(c => c.MailingCity).HasMaxLength(100);
        builder.Property(c => c.MailingState).HasMaxLength(2);
        builder.Property(c => c.MailingZip).HasMaxLength(10);

        builder.Property(c => c.AccountStatus).HasMaxLength(20).HasDefaultValue("Active");
        builder.Property(c => c.AccountCreatedDate).HasDefaultValueSql("GETDATE()");
        builder.Property(c => c.CreditScore).HasPrecision(5, 2);
        builder.Property(c => c.RiskTier).HasMaxLength(20);
        builder.Property(c => c.TotalPremiumInForce).HasPrecision(15, 2);

        builder.Property(c => c.CreatedDate).HasDefaultValueSql("GETDATE()");

        builder.HasMany(c => c.Quotes)
            .WithOne(q => q.Client)
            .HasForeignKey(q => q.ClientId);
    }
}
