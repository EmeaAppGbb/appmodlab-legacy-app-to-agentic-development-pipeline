using KeystoneInsurance.Modern.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KeystoneInsurance.Modern.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLog");
        builder.HasKey(a => a.AuditId);

        builder.Property(a => a.TableName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(10).IsRequired();
        builder.Property(a => a.FieldName).HasMaxLength(100);
        builder.Property(a => a.OldValue).HasMaxLength(2000);
        builder.Property(a => a.NewValue).HasMaxLength(2000);
        builder.Property(a => a.ChangedBy).HasMaxLength(100);
        builder.Property(a => a.ChangedDate).HasDefaultValueSql("GETDATE()");

        builder.HasIndex(a => new { a.TableName, a.RecordId });
        builder.HasIndex(a => a.ChangedDate);
    }
}
