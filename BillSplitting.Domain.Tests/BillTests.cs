using BillSplitting.Domain.Entities;
using FluentAssertions;

namespace BillSplitting.Domain.Tests;

public class BillTests
{
    [Fact]
    public void Constructor_ValidInputs_SetsProperties()
    {
        var bill = new Bill(100.555m, DateTime.UtcNow.AddDays(-1), "usd", "Dinner");
        bill.CurrencyCode.Value.Should().Be("USD");
        bill.Description.Should().Be("Dinner");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_InvalidAmount_Throws(decimal amount)
    {
        var act = () => new Bill(amount, DateTime.UtcNow.AddDays(-1), "USD");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_FutureDate_Throws(){
        var act = () => new Bill(100m, DateTime.UtcNow.AddDays(1), "USD");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_DescriptionOver50Chars_Throws()
    {
        var longDesc = new string('A', 51);
        var act = () => new Bill(50m, DateTime.UtcNow.AddDays(-1), "USD", longDesc);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddParticipant_ValidId_ReturnsTrue()
    {
        var bill = new Bill(100m, DateTime.UtcNow.AddDays(-1), "USD");
        var result = bill.AddParticipant(1);
        result.Should().BeTrue();
        bill.Participants.Should().HaveCount(1);
    }

    [Fact]
    public void AddParticipant_Duplicate_ReturnsFalse()
    {
        var bill = new Bill(100m, DateTime.UtcNow.AddDays(-1), "USD");
        bill.AddParticipant(1);
        var result = bill.AddParticipant(1);
        result.Should().BeFalse();
        bill.Participants.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddParticipant_InvalidId_Throws(int personId)
    {
        var bill = new Bill(100m, DateTime.UtcNow.AddDays(-1), "USD");
        var act = () => bill.AddParticipant(personId);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GenerateDebts_TwoParticipants_SplitsEvenly()
    {
        var bill = new Bill(100m, DateTime.UtcNow.AddDays(-1), "USD", "Dinner");
        bill.AddParticipant(1);
        bill.AddParticipant(2);
        typeof(Bill).GetProperty("Id")!.SetValue(bill, 1);
        var debts = bill.GenerateDebts();
        debts.Should().HaveCount(2);
        debts.Should().AllSatisfy(d =>
        {
            d.OriginalAmount.Should().Be(50m);
            d.CurrencyCode.Value.Should().Be("USD");
            d.Description.Should().Be("Dinner");
        });
    }

    [Fact]
    public void GenerateDebts_LessThanTwoParticipants_Throws()
    {
        var bill = new Bill(100m, DateTime.UtcNow.AddDays(-1), "USD");
        bill.AddParticipant(1);
        var act = () => bill.GenerateDebts();
        act.Should().Throw<InvalidOperationException>();
    }
}