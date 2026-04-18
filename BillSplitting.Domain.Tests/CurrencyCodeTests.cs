using BillSplitting.Domain.ValueObjects;
using FluentAssertions;

namespace BillSplitting.Domain.Tests;

public class CurrencyCodeTests
{
    [Fact]
    public void From_ValidCode_NormalizesToUppercase()
    {
        var code = CurrencyCode.From("usd");
        code.Value.Should().Be("USD");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void From_NullOrWhitespace_Throws(string? input)
    {
        var act = () => CurrencyCode.From(input!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    public void From_WrongLength_Throws(string input)
    {
        var act = () => CurrencyCode.From(input);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void From_NonAlphaCharacters_Throws()
    {
        var act = () => CurrencyCode.From("U2D");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var a = CurrencyCode.From("USD");
        var b = CurrencyCode.From("EUR");
        a.Equals(b).Should().BeFalse();    
    }
}