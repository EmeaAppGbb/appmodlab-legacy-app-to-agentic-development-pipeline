namespace KeystoneInsurance.Modern.Domain.ValueObjects;

public record PolicyNumber
{
    public string Value { get; }

    public PolicyNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Policy number cannot be empty.", nameof(value));
        Value = value;
    }

    public static PolicyNumber Generate(DateTime effectiveDate)
    {
        var datePart = effectiveDate.ToString("yyyyMMdd");
        var uniquePart = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
        return new PolicyNumber($"KIP{datePart}-{uniquePart}");
    }

    public override string ToString() => Value;
}
