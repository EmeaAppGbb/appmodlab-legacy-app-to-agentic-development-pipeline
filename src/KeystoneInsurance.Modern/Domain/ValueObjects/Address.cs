namespace KeystoneInsurance.Modern.Domain.ValueObjects;

public record Address(
    string Street,
    string City,
    string StateCode,
    string ZipCode,
    string? County = null);
