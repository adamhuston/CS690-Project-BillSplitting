namespace BillSplitting.Domain.ValueObjects;

public sealed class CurrencyCode: IEquatable<CurrencyCode>
{
    public string Value { get; }
    private CurrencyCode(string value)
    {
        Value = value;
    }

    public static CurrencyCode From(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Currency code is required.", nameof(input));
        }

        var normalized = input.Trim().ToUpperInvariant();

        if (normalized.Length != 3 || !normalized.All(char.IsLetter))
        {
            throw new ArgumentException("Currency code must be a 3-letter ISO code.", nameof(input));
        }

        return new CurrencyCode(normalized);
    }

    public override string ToString() => Value;

    public bool Equals(CurrencyCode? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is CurrencyCode other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
}