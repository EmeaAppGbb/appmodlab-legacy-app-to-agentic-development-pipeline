namespace KeystoneInsurance.Modern.Domain.Entities;

public class AuditLog
{
    public long AuditId { get; set; }
    public string TableName { get; set; } = null!;
    public int RecordId { get; set; }
    public string Action { get; set; } = null!;
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime ChangedDate { get; set; }
}
