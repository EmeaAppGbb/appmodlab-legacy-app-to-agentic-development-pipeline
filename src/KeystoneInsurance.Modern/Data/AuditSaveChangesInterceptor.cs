using KeystoneInsurance.Modern.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KeystoneInsurance.Modern.Data;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context!;
        var auditEntries = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Modified or EntityState.Added or EntityState.Deleted)
            .Where(e => e.Entity is not AuditLog))
        {
            var tableName = entry.Metadata.GetTableName() ?? entry.Metadata.Name;
            var primaryKey = entry.Properties
                .FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue;
            var recordId = primaryKey is int id ? id : 0;

            if (entry.State == EntityState.Modified)
            {
                foreach (var prop in entry.Properties.Where(p => p.IsModified))
                {
                    auditEntries.Add(new AuditLog
                    {
                        TableName = tableName,
                        RecordId = recordId,
                        Action = "UPDATE",
                        FieldName = prop.Metadata.Name,
                        OldValue = prop.OriginalValue?.ToString(),
                        NewValue = prop.CurrentValue?.ToString(),
                        ChangedDate = DateTime.UtcNow
                    });
                }
            }
            else
            {
                auditEntries.Add(new AuditLog
                {
                    TableName = tableName,
                    RecordId = recordId,
                    Action = entry.State == EntityState.Added ? "INSERT" : "DELETE",
                    ChangedDate = DateTime.UtcNow
                });
            }
        }

        if (auditEntries.Count > 0)
        {
            context.Set<AuditLog>().AddRange(auditEntries);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
